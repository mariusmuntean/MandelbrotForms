namespace Mandelbrot.Models
{
    public struct Complex
    {
            public Complex(float  re, float im)
        {
            Re = re;
            Im = im;
        }

        public float Re { get; set; }
        public float Im { get; set; }
    }
}