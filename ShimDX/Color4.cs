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

        public float[] ToArray()
        {
            return new float[]{Red,Green,Blue,Alpha};
        }

        public float Luminance()
        {
            return (float)(Red * 0.2125 + Green * 0.7154 + Blue * 0.0721);
        }
     }
}