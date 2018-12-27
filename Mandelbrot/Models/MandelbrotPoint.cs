namespace Mandelbrot.Models
{
    public struct MandelbrotPoint
    {
        /// <summary>
        /// The complex number
        /// </summary>
        public Complex Complex { get; set; }

        /// <summary>
        /// Iteration count until escape or cutoff
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// The square of the distance from the complex number to the origin of the complex plane.
        /// </summary>
        public float DistanceToCenterSquared { get; set; }
    }
}