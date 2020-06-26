// BEWARE: GDImport specific shim for Gui.cs

using SlimDX;

namespace SB3Utility
{
    public class Renderer
    {
        public SlimDX.Direct3D11.Device Device;
    }
    public static class Gui
    {
        public static Renderer Renderer = new Renderer();
    }
}