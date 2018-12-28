using System.Collections.Generic;

namespace Mandelbrot.Models
{
    /// <summary>
    /// Holds the iteration count (n) for each complex plane point
    /// </summary>
    public class Mandelbrot
    {
        /// <summary>
        /// The points and their iteration count
        /// </summary>
        public List<MandelbrotPoint> Points { get; set; }

        /// <summary>
        /// The width (distance from largest Re to smallest Re) of the points
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height (distance from largest Im to smallest Im) of the points
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The top-left most point
        /// </summary>
        public Complex TopLeft { get; set; }

        /// <summary>
        /// The bottom-right most point
        /// </summary>
        public Complex BottomRight { get; set; }
    }
}