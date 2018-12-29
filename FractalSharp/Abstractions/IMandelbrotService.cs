using System.Threading.Tasks;
using FractalSharp.Models;

namespace FractalSharp.Abstractions
{
    public interface IMandelbrotService
    {
        /// <summary>
        /// Produces <see cref="MandelbrotPoint"/> for each point in the desired complexPlaneArea.
        /// <para>The <see cref="MandelbrotPoint"/>s are produced in batches and each batch is passed to the provided handler</para>
        /// </summary>
        /// <param name="complexPlaneArea">The display area to be mapped to the Mandelbrot range, i.e. -2.5 < x < 1 and -1 < y < 1</param>
        /// <param name="stepSize"></param>
        /// <returns></returns>
        Task<Models.Mandelbrot> ProduceDisplayPoints(ComplexPlaneArea complexPlaneArea);
    }
}