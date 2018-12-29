using System.Collections.Generic;
using System.Threading.Tasks;
using FractalSharp.Abstractions;
using FractalSharp.Models;
using FractalSharp.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(PureMandelbrotService))]

namespace FractalSharp.Services
{
    public class PureMandelbrotService : IMandelbrotService
    {
        public const int MaxIterations = 100;

        /// <summary>
        /// Produces <see cref="MandelbrotPoint"/> for each point in the desired complexPlaneArea.
        /// <para>The <see cref="MandelbrotPoint"/>s are produced in batches and each batch is passed to the provided handler</para>
        /// </summary>
        /// <param name="complexPlaneArea">The display area to be mapped to the Mandelbrot range, i.e. -2.5 < x < 1 and -1 < y < 1</param>
        /// <returns></returns>
        public Task<Models.Mandelbrot> ProduceDisplayPoints(ComplexPlaneArea complexPlaneArea)
        {
            var stepSize = complexPlaneArea.StepSize;
            return Task.Run(() =>
            {
                var displayPoints = new List<MandelbrotPoint>();

                for (var re = complexPlaneArea.MinRe; re < complexPlaneArea.MaxRe; re += complexPlaneArea.StepSize)
                {
                    for (var im = complexPlaneArea.MinIm; im < complexPlaneArea.MaxIm; im += stepSize)
                    {
                        var mandelOrbitX = 0.0f;
                        var mandelOrbitY = 0.0f;
                        var currentIteration = 0;

                        while (mandelOrbitX * mandelOrbitX + mandelOrbitY * mandelOrbitY <= 2 * 2
                               && currentIteration < MaxIterations)
                        {
                            var mandelOrbitTempX = mandelOrbitX * mandelOrbitX - mandelOrbitY * mandelOrbitY + re;
                            mandelOrbitY = 2 * mandelOrbitX * mandelOrbitY + im;
                            mandelOrbitX = mandelOrbitTempX;
                            currentIteration++;
                        }

                        displayPoints.Add(new MandelbrotPoint
                        {
                            Complex = new Complex(re, im),
                            Iterations = currentIteration,
                            DistanceToCenterSquared = mandelOrbitX * mandelOrbitX + mandelOrbitY * mandelOrbitY
                        });
                    }
                }


                return new Models.Mandelbrot
                {
                    Points = displayPoints,
                    TopLeft = new Complex(complexPlaneArea.MinRe, complexPlaneArea.MaxIm),
                    BottomRight = new Complex(complexPlaneArea.MaxRe, complexPlaneArea.MinIm),
                    Width = (int) ((complexPlaneArea.MaxRe - complexPlaneArea.MinRe) / complexPlaneArea.StepSize),
                    Height = (int) ((complexPlaneArea.MaxIm - complexPlaneArea.MinIm) / complexPlaneArea.StepSize)
                };
            });
        }
    }
}