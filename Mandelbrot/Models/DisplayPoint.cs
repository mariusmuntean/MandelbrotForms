namespace Mandelbrot.Models
{
    public struct DisplayPoint
    {
        public Coordinates Coordinates { get; set; }

        public int Iterations { get; set; }

        public float DistanceToCenter { get; set; }
    }
}