using System;

namespace Mandelbrot.Models
{
    public struct ComplexPlaneArea
    {
        private const float Epsilon = 0.0001f;

        public ComplexPlaneArea(float horizontalMin,
            float horizontalMax,
            float verticalMin,
            float verticalMax)
        {
            MaxRe = horizontalMax;
            MinRe = horizontalMin;
            MaxIm = verticalMax;
            MinIm = verticalMin;
        }

        public float MinRe { get; set; }
        public float MaxRe { get; set; }

        public float RealRangeWidth => MaxRe - MinRe;

        public float MinIm { get; set; }
        public float MaxIm { get; set; }

        public float ImaginaryRangeHeight => MaxIm - MinIm;

        public static ComplexPlaneArea None = new ComplexPlaneArea(0, 0, 0, 0);

        public override bool Equals(object obj)
        {
            if (obj is ComplexPlaneArea complexPlaneArea)
            {
                return Math.Abs(MinRe - complexPlaneArea.MinRe) < Epsilon
                       && Math.Abs(MaxRe - complexPlaneArea.MaxRe) < Epsilon
                       && Math.Abs(MinIm - complexPlaneArea.MinIm) < Epsilon
                       && Math.Abs(MaxIm - complexPlaneArea.MaxIm) < Epsilon;
            }

            return base.Equals(obj);
        }
    }
}