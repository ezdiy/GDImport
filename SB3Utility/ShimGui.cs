// BEWARE: GDImport specific shim for Gui.cs

using SlimDX;

namespace SB3Utility
{
    public class Renderer
    {
        public GraphicsDevice Device;
    }
    public static class Gui
    {
        public static Renderer Renderer = new Renderer();
    }
}