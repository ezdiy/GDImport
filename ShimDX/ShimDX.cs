// We implement only as much to make fmod/d3d-specific parts of sb3u compile, but that doesn't mean the shims always
// do something. We don't care for much of gui and texture/sound conversion stuff, but the D3D math must be in
// place (FNA bits).
//
// While it's possible to rip out the windows specific parts, that would complicate future backports.
// With a shim it's plain drag&drop.

using System;
using System.IO;

namespace SlimDX
{
    #region Globals
    public class Result
    {
        public int Code;
        public string Name;
        public string Description;
    }

    public class DataStream : IDisposable
    {
        public DataStream(byte[] data, bool a, bool b)
        {
            throw new NotImplementedException();
        }
        public void WriteRange<T>(T[] data)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
        }
    }
    #endregion

    #region DX11 cruft
    namespace Direct3D11
    {
        public class Device
        {
            public Device ImmediateContext;
        }
        public enum ImageFileFormat
        {
            Bmp,
            Tga,
            Dds,
        }

        public class Texture2D
        {
            public static Texture2D FromStream(Device device, Stream stream, int sizeInBytes, ImageLoadInformation loadInfo)
            {
                throw new NotImplementedException();
            }

            public static Texture2D FromMemory(Device device, byte[] data, ImageLoadInformation loadInfo)
            {
                throw new NotImplementedException();
            }

            public static Result ToStream(Device device, Texture2D texture, ImageFileFormat format, Stream stream)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
            }
        }

        public enum ResourceDimension
        {
            Unknown,
            Buffer,
            Texture1D,
            Texture2D,
            Texture3D,
        }

        public class Direct3D11Exception : Exception
        {
            public Result ResultCode;
        }

        public enum ResourceUsage
        {
            Staging
        }

        public enum ResourceOptionFlags
        {
            None
        }

        public enum FilterFlags
        {
            None
        }

        public enum CpuAccessFlags
        {
            Read
        }

        public enum BindFlags
        {
            None
        }

        public class ImageLoadInformation
        {
            public ResourceUsage Usage;
            public ResourceOptionFlags OptionFlags;
            public DXGI.Format Format;
            public int MipLevels;
            public FilterFlags MipFilterFlags, FilterFlags;
            public CpuAccessFlags CpuAccessFlags;
            public BindFlags BindFlags;
        }
    }
    #endregion
    
    #region DX9 cruft

    namespace Direct3D9
    {
        public enum Format
        {
            A32B32G32R32F = 116,
            A16B16G16R16F = 113,
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
    #endregion
    
    #region DXGI
    namespace DXGI
    {
        public enum Format
        {
            R8G8B8A8_UNorm = 28,
            BC7_UNorm = 98,
            BC7_UNorm_SRGB = 99,
        }
    }
    #endregion
}