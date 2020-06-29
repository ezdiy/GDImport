using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GLTFSchema
{
    public partial class Gltf : GltfNodeBase
    {
        public const int Unset = -1;        // Unset integers and enums are initialized to this.
        #region Inner schema definition
        public class Asset : GltfNodeBase
        {
            public string copyright, generator, version, minVersion;
        }
        public class Buffer : GltfUri
        {
            public int byteLength;
        }
        public class BufferView : GltfNamedNodeBase
        {
            public int buffer, byteOffset, byteLength, byteStride, target;
        }
        public class Camera
        {
            public class Perspective : GltfNodeBase
            {
                public float aspectRatio, yfov, zfar, znear;
            }
            public class Orthographic : GltfNodeBase
            {
                public float xmag, ymag, zfar, znear;
            }
            public Perspective perspective;
            public Orthographic orthographic;
            public string type;
        }
        public class Image : GltfUri
        {
            public string mimeType;
            public int bufferView;
        }
        public class Material : GltfNamedNodeBase
        {
            #region Inside material schema defs
            public class Texture
            {
                [DefaultValue(-1)] public int index;
                public int texCoord;
            }
            public class NormalTexture : Texture
            {
                [DefaultValue(1.0)] public float scale = 1;
            }
            public class OcclusionTexture : Texture
            {
                [DefaultValue(1.0)] public float strength = 1;
            }
            public class PbrMetallicRoughness : GltfNodeBase
            {
                public float[] baseColorFactor;
                public Texture baseColorTexture, metallicRoughnessTexture;
                [DefaultValue(1.0)] public float metallicFactor;
                [DefaultValue(1.0)] public float roughnessFactor;
            }
            public class PbrSpecularGlossiness : GltfNodeBase
            {
                public float[] diffuseFactor;
                public Texture diffuseTexture;
                public float[] specularFactor;
                [DefaultValue(1.0)] public float glossinessFactor = 1;
                public Texture specularGlossinessTexture;
            }
            #endregion
            public NormalTexture normalTexture;
            public Texture emissiveTexture;
            public OcclusionTexture occlusionTexture;
            public float[] emissiveFactor;
            [DefaultValue("OPAQUE")] public string alphaMode;
            [DefaultValue(0.5)] public float alphaCutoff;
            public bool doubleSided;
            public PbrMetallicRoughness PbrMetallicRoughnessMetallicRoughness;
        }
        public class Mesh : GltfNamedNodeBase
        {
            public List<Primitive> primitives;
        }
        public class Node : GltfNamedNodeBase
        {
            [DefaultValue(Unset)] public int camera = Unset;
            public List<int> children;
            [DefaultValue(Unset)] public int skin = Unset;
            public float[] matrix;
            [DefaultValue(Unset)] public int mesh = Unset;
            public float[] rotation, scale, translation, weights;
        }
        public class Primitive : GltfNodeBase
        {
            public enum Mode
            {
                POINTS, LINES, LINE_LOOP, LINE_STRIP, TRIANGLES, TRIANGLE_STRIP, TRIANGLE_FAN
            }
            public Dictionary<string, int> attributes;
            [DefaultValue(Unset)] public int indices = Unset;
            [DefaultValue(Unset)] public int material = Unset;
            [DefaultValue(Mode.TRIANGLES)] public Mode mode = Mode.TRIANGLES;
            public List<int> targets;
        }
        public class Sampler : GltfNamedNodeBase
        {
            [DefaultValue(Filter.Unset)] public Filter magFilter = Filter.Unset;
            [DefaultValue(Filter.Unset)] public Filter minFilter = Filter.Unset;
            [DefaultValue(Wrap.REPEAT)] public Wrap wrapS = Wrap.REPEAT;
            [DefaultValue(Wrap.REPEAT)] public Wrap wrapT = Wrap.REPEAT;
        }
        public class Scene : GltfNamedNodeBase
        {
            public List<int> nodes;
        }
        public class Skin : GltfNamedNodeBase
        {
            public string name;
            public string inverseBindMatrices;
            [DefaultValue(Unset)] public int skeleton = Unset;
            public List<int> joints;
        }
        public class Target : GltfNodeBase
        {
            [DefaultValue(Unset)] public int node = Unset;
            public string path;
        }
        public class Texture : GltfNamedNodeBase
        {
            [DefaultValue(Unset)] public int source = Unset;
            [DefaultValue(Unset)] public int sampler = Unset;
        }
        public class Accessor : GltfNamedNodeBase
        {
            public class Sparse : GltfNodeBase
            {
                public class Value : GltfNodeBase
                {
                    public int bufferView;
                    [DefaultValue(0)]
                    public int byteOffset = 0;
                }
                public int count;
                public List<int> indices;
                public List<Value> values;
            }
            [DefaultValue(Unset)] public int bufferView = Unset;
            [DefaultValue(0)] public int byteOffset = 0;
            public ComponentType componentType;
            [DefaultValue(false)] public bool normalized = false;
            public int count;
            public string type;
            public float[] max, min;
            public Sparse sparse;
        }

        public class Animation : GltfNamedNodeBase
        {
            public class Channel : GltfNodeBase
            {
                public int sampler, target;
            }
            public class Sampler : GltfNodeBase
            {
                public int input, output;
                [DefaultValue("LINEAR")] public string interpolation;
            }
            public List<Channel> channels;
            public List<Sampler> samplers;
        }
        #endregion

        public List<Accessor> accessors;
        public List<Animation> animations;
        public Asset asset = new Asset(); // Mandatory
        public List<Buffer> buffers;
        public List<BufferView> bufferViews;
        public List<Camera> cameras;
        public List<Material> materials;
        public List<Image> images;
        public List<Mesh> meshes;
        public List<Node> nodes;
        public List<Sampler> samplers;
        [DefaultValue(-1)]
        public int scene;
        public List<Scene> scenes;
        public List<Skin> skins;
        public List<Texture> textures;
    }
    #region Attributes common to most schemas
    public class GltfNodeBase
    {
        public Dictionary<string, object> extensions;
        public object extras;
    }
    public class GltfNamedNodeBase : GltfNodeBase
    {
        public string name;
    }
    public class GltfUri : GltfNamedNodeBase
    {
        public string uri;
    }
    #endregion
    
    #region gl ENUMs
    public enum Filter
    {
        Unset = -1,
        NEAREST = 9728,
        LINEAR,
        NEAREST_MIPMAP_NEAREST = 9984,
        LINEAR_MIPMAP_NEAREST,
        NEAREST_MIPMAP_LINEAR,
        LINEAR_MIPMAP_LINEAR,
    }
    public enum Wrap
    {
        CLAMP_TO_EDGE = 33071,
        MIRRORED_REPEAT = 33648,
        REPEAT = 10497,
    }
    public enum ComponentType
    {
        BYTE = 5120,
        UNSIGNED_BYTE,
        SHORT,
        UNSIGNED_SHORT,
        UNSIGNED_INT = 5125,
        FLOAT,
    }
    #endregion
}
