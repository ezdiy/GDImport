using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GLTFSchema;
using Newtonsoft.Json;
using SB3Utility;
using SlimDX;

namespace GDImport
{
    public class XXGLTFWriter : Gltf
    {
        public class matRecord
        {
            public int index;
            public string hash;
        }
        BinaryWriter vertexOutput, indicesOutput;
        BufferWriter indicesBuffer, vertexBuffer;
        BufferWriter.View indicesView, vertexView;
        private string dstDir, dstName;
        private xxParser xxParser;
        private Dictionary<string, List<matRecord>> mat2idx = new Dictionary<string, List<matRecord>>();
        private Dictionary<string, int> tex2idx = new Dictionary<string, int>();

        static string mat2s(object a)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            serializer.Formatting = Formatting.Indented;
            var at = new StringWriter();
            serializer.Serialize(at, a);
            return at.ToString();
        }
        
        public XXGLTFWriter(string dir, string name)
        {
            asset.version = "2.0";

            dstDir = dir;
            dstName = name;
            vertexOutput = new BinaryWriter(File.Create(Path.Join(dstDir, dstName, "vertex.bin")));
            indicesOutput = new BinaryWriter(File.Create(Path.Join(dstDir, dstName, "index.bin")));

            vertexBuffer = NewBuffer();
            vertexBuffer.buf.uri = dstName + "/vertex.bin";
            indicesBuffer = NewBuffer();
            indicesBuffer.buf.uri = dstName + "/index.bin";

            vertexView = vertexBuffer.NewView();
            indicesView = indicesBuffer.NewView();
        }

        public string textureName(string name)
        {
            var opaque = "";
            if (name.EndsWith(".bmp"))
                opaque = "_opaque";
            return dstName + "/" + Path.GetFileNameWithoutExtension(name) + opaque + ".png";
        }
        public Nullable<int> getTexture(string name)
        {
            if (name == null || name == "")
                return null;
            if (!tex2idx.ContainsKey(name))
            {
                images ??= new List<Image>();
                images.Add(new Image()
                {
                    uri = textureName(name)
                });
                textures ??= new List<Texture>();
                tex2idx[name] = textures.Count;
                textures.Add(new Texture()
                {
                    source = images.Count-1,
                });
            }
            return tex2idx[name];
        }

        public Gltf.Material.Texture getMatTexture(xxMaterialTexture xt)
        {
            if (xt == null) return null;
            var t = getTexture(xt.Name);
            if (t == null) return null;
            return new Gltf.Material.Texture()
            {
                index = t.Value,
            };
        }

        public Gltf.Material buildMaterial(string nm, xxMaterial om)
        {
            return new Material()
            {
                extras = new
                {
                    flags = om.Unknown1,
                    // shader-specific textures
                    tex1 = getTexture(om.Textures[1].Name),
                    tex2 = getTexture(om.Textures[2].Name),
                },
                name = nm,
                extensions = new Dictionary<string, object>()
                {
                    ["KHR_materials_pbrSpecularGlossiness"] = new Gltf.Material.PbrSpecularGlossiness()
                    {
                        diffuseFactor = om.Diffuse.ToArray(),
                        diffuseTexture = getMatTexture(om.Textures[0]),
                        specularFactor = om.Specular.ToArray(),
                        specularGlossinessTexture = getMatTexture(om.Textures[3]),
                        glossinessFactor = om.Power / 100,
                    }
                },
            };
        }

        public int getMaterial(int idx)
        {
            var om = xxParser.MaterialList[idx];
            var nm = om.Name;
            var ms = new MemoryStream();
            om.WriteTo(ms);
            var hash = Convert.ToBase64String(MD5.Create().ComputeHash(ms.ToArray()));

            // not found, add list
            if (!mat2idx.TryGetValue(nm, out List<matRecord> ml))
            {
                ml = new List<matRecord>();
                mat2idx[nm] = ml;
            }

            // possibly existing, version under specific index
            foreach (var mr in ml)
            {
                if (mr.hash == hash)
                    return mr.index;
            }

            // otherwise build
            var suffix = "";
            if (ml.Count > 0)
            {
                // same name, but different version
                suffix = "_v" + (ml.Count + 1);
            }
            var mat = buildMaterial(nm + suffix, om);
            materials ??= new List<Material>();
            ml.Add(new matRecord()
                {
                    index = materials.Count,
                    hash = hash,
                }
            );
            materials.Add(mat);
            return ml.Last().index;
        }

        public void AddScene(xxParser xx)
        {
            scenes ??= new List<Scene>();
            var sc = new Scene();
            sc.name = xx.Name;
            xxParser = xx;
            // Extract material textures
            foreach (var xxTex in xx.TextureList)
            {
                if (xxTex.Name.ToLower().EndsWith(".bmp"))
                {
                    xxTex.ImageData[0] = (byte)'B';
                    xxTex.ImageData[1] = (byte)'M';
                }
                //Console.WriteLine("{0}: {1} in {2}", xxTex.Name, xxTex.ImageData.Length, xxParser.Name);
                SaveTexture(new MemoryStream(xxTex.ImageData), xxTex.Name);
            }
            // Recursively export the scene
            saveFrame(xx.Frame, ref sc.nodes);
            scenes.Add(sc);
        }
        
        void saveFrame(xxFrame f, ref List<int> children)
        {
            // One frame is gltf node with a single mesh
            var node = NewNode(ref children);
            node.extras = new
            {
                flags = f.Unknown1,
                flags2 = f.Unknown2,
            };
            node.name = f.Name;
            node.matrix = f.Matrix!=Matrix.Identity?f.Matrix.ColumnMajor():null;
            
            var mesh = NewMesh(out int meshidx);
            mesh.name = f.Name;

            if (f.Mesh != null)
            {
                node.mesh = meshidx;

                // Construct a primitive template shared by all primitives (submeshes) under this mesh.
                var vertexLen = 0;
                var acc = new int[5];
                var attrs = new Dictionary<string, int>()
                {
                    ["POSITION"] = acc[0] = vertexView.NewAccessor("VEC3", ComponentType.FLOAT, ref vertexLen),
                    ["WEIGHTS_0"] = acc[1] = vertexView.NewAccessor("VEC3", ComponentType.FLOAT, ref vertexLen),
                    ["JOINTS_0"] = acc[2] = vertexView.NewAccessor("VEC4", ComponentType.BYTE, ref vertexLen),
                    ["NORMAL"] = acc[3] = vertexView.NewAccessor("VEC3", ComponentType.FLOAT, ref vertexLen),
                    ["TEXCOORD_0"] = acc[4] = vertexView.NewAccessor("VEC2", ComponentType.FLOAT, ref vertexLen),
                };
                accessors[acc[0]].min = f.Bounds.Min.ToArray();
                accessors[acc[0]].max = f.Bounds.Max.ToArray();
                vertexView.view.byteStride = vertexLen;

                // Instantiate submeshes as primitives
                foreach (var sub in f.Mesh.SubmeshList)
                {
                    // The indices always get a new accessor
                    var iAcc = indicesView.NewAccessor("SCALAR", ComponentType.UNSIGNED_SHORT, sub.FaceList.Count);

                    // Build the primitive
                    mesh.primitives ??= new List<Primitive>();
                    mesh.primitives.Add(new Primitive()
                    {
                        extras = new
                        {
                            flags = sub.Unknown1,
                            flags2 = sub.Unknown2,
                            flags3 = sub.Unknown3,
                            flags4 = sub.Unknown4,
                            flags5 = sub.Unknown5,
                            flags6 = sub.Unknown6,
                        },
                        material = getMaterial(sub.MaterialIndex),
                        indices = iAcc,
                        attributes = attrs,
                    });

                    // Save indices
                    foreach (var ind in sub.FaceList)
                    {
                        indicesOutput.Write(ind.VertexIndices);
                        accessors[iAcc].count += 3;
                        indicesBuffer.buf.byteLength += 6;
                        indicesView.view.byteLength += 6;
                    }

                    // Update vertex attribute accessors with vertex count
                    for (var i = 0; i < acc.Length; i++)
                    {
                        accessors[acc[i]].count += sub.VertexList.Count;
                    }

                    // And dump vertices
                    foreach (var vert in sub.VertexList)
                    {
                        vertexOutput.Write(vert.Position);
                        vertexOutput.Write(vert.Weights3);
                        vertexOutput.Write(vert.BoneIndices);
                        vertexOutput.Write(vert.Normal);
                        vertexOutput.Write(vert.UV);
                        vertexBuffer.buf.byteLength += vertexLen;
                        vertexView.view.byteLength += vertexLen;
                    }
                }
            }

            // Recurse into children
            foreach (var child in f)
            {
                saveFrame(child, ref node.children);
            }
        }
        
        // Serialize the output json.
        public void Save()
        {
            var outJs = File.CreateText(Path.Join(dstDir, dstName) + ".gltf");
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            serializer.Formatting = Formatting.Indented;
            serializer.Serialize(outJs, this);
            outJs.Close();
            vertexOutput.Close();
            indicesOutput.Close();
        }

        public void SaveTexture(Stream data, string name)
        {
            var tName = dstDir + textureName(name);
            var fi = new FileInfo(tName);
            if (name.ToLower().EndsWith(".bmp"))
            {
                using var bmp = new Bitmap(data);
                bmp.Save(tName, ImageFormat.Png);
            }
            else
            {
                using var image = Pfim.Pfim.FromStream(data);
                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                try
                {
                    using var tga = new Bitmap(image.Width, image.Height, image.Stride, PixelFormat.Format32bppArgb,
                        Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0));
                    tga.Save(tName, ImageFormat.Png);
                }
                finally
                {
                    handle.Free();
                }
                image.Dispose();
            }
            var nfi = new FileInfo(tName);
            if (fi != null)
            {
                if (fi.Length != nfi.Length)
                {
                    Console.WriteLine("{0} mismatch, {1} != {2}", name, fi.Length, nfi.Length);
                }
            }
        }
    }
}    