using System.Drawing;

namespace SlimDX
{
    public class Color4
    {
        public float Red, Green, Blue, Alpha;

        public Color4(){}
        public Color4(float R, float G, float B, float A)
        {
            Red = R;
            Green = G;
            Blue = B;
            Alpha = A;
        }
    }
}