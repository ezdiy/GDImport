using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace GLTFSchema
{
    public partial class Gltf
    {
        // Register a new mesh, and return its index
        public Mesh NewMesh(out int index)
        {
            meshes ??= new List<Mesh>();
            var mesh = new Mesh();
            index = meshes.Count;
            meshes.Add(mesh);
            return mesh;
        }
        
        // Register a new node, and append its index to supplied children array
        public Node NewNode(ref List<int>children)
        {
            nodes ??= new List<Node>();
            children ??= new List<int>();
            children.Add(nodes.Count);
            var node = new Node();
            nodes.Add(node);
            return node;
        }

        // Register a new buffer objects (you need to make views over it)
        public BufferWriter NewBuffer()
        {
            buffers ??= new List<Buffer>();
            var buf = new BufferWriter() { g=this,index=buffers.Count};
            buffers.Add(buf.buf);
            return buf;
        }

        public static int GetSize(string type, ComponentType ct)
        {
            var tSize = new Dictionary<string, int>()
            {
                ["VEC4"] = 4,
                ["VEC3"] = 3,
                ["VEC2"] = 2,
                ["SCALAR"] = 1,
            };
            var ctSize = new Dictionary<ComponentType, int>()
            {
                [ComponentType.BYTE] = 1,
                [ComponentType.UNSIGNED_BYTE] = 1,
                [ComponentType.SHORT] = 2,
                [ComponentType.UNSIGNED_SHORT] = 2,
                [ComponentType.FLOAT] = 4,
                [ComponentType.UNSIGNED_INT] = 4,
            };
            return tSize[type] * ctSize[ct];
        }

        public class BufferWriter
        {
            public Buffer buf = new Buffer();
            public Gltf g { get; set; }
            public int index { get; set; }

            public class View
            {
                public BufferView view = new BufferView();
                public BufferWriter bw { get; set; }
                public int index { get; set; }

                // Create new accessor. count need to be fixed up later.
                public int NewAccessor(string type, ComponentType ct, ref int offset)
                {
                    var a = new Accessor()
                    {
                        bufferView = index,
                        byteOffset = view.byteLength + offset,
                        componentType = ct,
                        type = type,
                    };
                    bw.g.accessors ??= new List<Accessor>();
                    bw.g.accessors.Add(a);
                    offset += GetSize(type, ct);
                    return bw.g.accessors.Count-1;
                }

                public int NewAccessor(string type, ComponentType ct, int count)
                {
                    var dummy = 0;
                    return NewAccessor(type, ct, ref dummy);
                }
                
            }
            // Create a new view of this buffer. byteOffset and byteLength need to be fixed up.
            public View NewView()
            {
                g.bufferViews ??= new List<BufferView>();
                var v = new View() {bw = this, index = g.bufferViews.Count};
                g.bufferViews.Add(v.view);
                return v;
            }
        }
    }
}
