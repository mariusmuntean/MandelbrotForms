namespace Mandelbrot.Models
{
    public struct Range
    {
        public Range(int horizontalMin,
            int horizontalMax,
            int verticalMin,
            int verticalMax)
        {
            HorizontalMax = horizontalMax;
            HorizontalMin = horizontalMin;
            VerticalMax = verticalMax;
            VerticalMin = verticalMin;
        }

        public int HorizontalMin { get; set; }
        public int HorizontalMax { get; set; }

        public int Width => HorizontalMax - HorizontalMin;

        public int VerticalMin { get; set; }
        public int VerticalMax { get; set; }

        public int Height => VerticalMax - VerticalMin;
    }
}