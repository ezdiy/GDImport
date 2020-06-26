using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

// We implement only as much to make fmod/d3d-specific parts for sb3u compile, but that doesn't mean the shims always
// do something. We don't care for much of gui and texture/sound conversion stuff, but the D3D math must be in
// place (FNA bits).
//
// While it's possible to rip out the windows specific parts of sb3u, that would complicate future backports.
// With a shim it's plain drag&drop.

namespace SlimDX
{
    public class GraphicsDevice {}

    namespace Direct3D11
    {
        public class Device {}
    }

    namespace DXGI
    {
        public class Dummy
        {
        }

        public enum Format
        {
            R8G8B8A8_UNorm = 28,
            BC7_UNorm = 98,
            BC7_UNorm_SRGB = 99,
        }
    }

    namespace Direct3D9
    {
        public enum Format
        {
            A32B32G32R32F = 116,
            A16B16G16R16F = 113,
        }
    }

    public enum ImageFileFormat
    {
        Bmp,
        Tga,
    }
    

    public enum ResourceType
    {
    }

    public class ImageInformation
    {
        public int Width, Height, Depth;
        public ImageFileFormat ImageFileFormat;
        public Format Format;
        public int MipLevels;
        public ResourceType ResourceType;
        
        public static ImageInformation FromMemory(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}