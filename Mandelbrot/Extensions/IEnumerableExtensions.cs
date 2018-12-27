using System.Collections.Generic;
using System.Linq;

namespace Mandelbrot.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> GetChunks<T>(this IEnumerable<T> source, int chunkSize)
        {
            while (source.Any())
            {
                yield return source.Take(chunkSize);
                source = source.Skip(chunkSize);
            }
        }

        public static IEnumerable<List<T>> GetListChunks<T>(this IEnumerable<T> source, int chunkSize)
        {
            var sourceClone = source.ToList();
            while (sourceClone.Any())
            {
                yield return sourceClone.Take(chunkSize).ToList();
                sourceClone = sourceClone.Skip(chunkSize).ToList();
            }
        }
    }
}