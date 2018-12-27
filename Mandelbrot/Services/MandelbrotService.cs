//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Mandelbrot.Models;
//
//namespace Mandelbrot.Services
//{
//    public class MandelbrotService
//    {
//        public const int MaxIterations = 100;
//
//        /// <summary>
//        /// Produces <see cref="MandelbrotPoint"/> for each point in the desired partition.
//        /// <para>The <see cref="MandelbrotPoint"/>s are produced in batches and each batch is passed to the provided handler</para>
//        /// </summary>
//        /// <param name="originalRange">The display area to be mapped to the Mandelbrot range, i.e. -2.5 < x < 1 and -1 < y < 1</param>
//        /// <param name="originalRangePartition">A partition within the original range. <see cref="MandelbrotPoint"/>s will be produced only for this partition</param>
//        /// <param name="OnPartitionSliceReady">Handler to be invoked each time a new batch of <see cref="MandelbrotPoint"/>s is ready</param>
//        /// <returns></returns>
//        public async Task ProduceDisplayPoints(ComplexPlaneArea originalRange,
//            ComplexPlaneArea originalRangePartition,
//            Action<List<MandelbrotPoint>> OnPartitionSliceReady)
//        {
//            var displayPoints = new List<MandelbrotPoint>();
//
//            for (var pixX = originalRangePartition.MinRe; pixX < originalRangePartition.MaxRe; pixX++)
//            {
//                for (var pixY = originalRangePartition.MinIm; pixY < originalRangePartition.MaxIm; pixY++)
//                {
//                    // Map pixel coordinates to Mandelbrot scale, i.e. -2.5 < x < 1 and -1 < y 1 
//                    var mandelX = MapToHorizontalMandelbrotScale(pixX,
//                        originalRange.MinRe,
//                        originalRange.MaxRe);
//                    var mandelY = MapToVerticalMandelbrotScale(pixY,
//                        originalRange.MinIm,
//                        originalRange.MaxIm);
//
//                    var mandelOrbitX = 0.0f;
//                    var mandelOrbitY = 0.0f;
//                    var currentIteration = 0;
//
//                    while (mandelOrbitX * mandelOrbitX + mandelOrbitY * mandelOrbitY <= 2 * 2
//                           && currentIteration < MaxIterations)
//                    {
//                        var mandelOrbitTempX = mandelOrbitX * mandelOrbitX - mandelOrbitY * mandelOrbitY + mandelX;
//                        mandelOrbitY = 2 * mandelOrbitX * mandelOrbitY + mandelY;
//                        mandelOrbitX = mandelOrbitTempX;
//                        currentIteration++;
//                    }
//
//                    displayPoints.Add(new MandelbrotPoint
//                    {
//                        Complex = new Complex(pixX, pixY),
//                        Iterations = currentIteration,
//                        DistanceToCenterSquared = (float) Math.Sqrt(mandelOrbitX * mandelOrbitX + mandelOrbitY * mandelOrbitY)
//                    });
//                }
//            }
//
//            OnPartitionSliceReady?.Invoke(displayPoints);
//        }
//
//        private float MapToVerticalMandelbrotScale(int x, float rangeStart, float rangeEnd)
//        {
//            return MapValueFromRangeToRange(x,
//                rangeStart,
//                rangeEnd,
//                -1,
//                1);
//        }
//
//        private float MapToHorizontalMandelbrotScale(int y, float rangeStart, float rangeEnd)
//        {
//            return MapValueFromRangeToRange(y,
//                rangeStart,
//                rangeEnd,
//                -2.5f,
//                1.0f);
//        }
//
//        private static float MapValueFromRangeToRange(int val,
//            float originalRangeMin,
//            float originalRangeMax,
//            float newRangeMin,
//            float newRangeMax)
//        {
//            if (val < originalRangeMin)
//            {
//                return newRangeMin;
//            }
//
//            if (val > originalRangeMax)
//            {
//                return newRangeMax;
//            }
//
//            var originalRangeMagnitude = Math.Abs(originalRangeMax - originalRangeMin);
//            var valueMagnitude = Math.Abs(val - originalRangeMin);
//            var rangePercentage = valueMagnitude / originalRangeMagnitude;
//
//            var newRangeValue = newRangeMin + Math.Abs(newRangeMax - newRangeMin) * rangePercentage;
//
//            return newRangeValue;
//        }
//    }
//}