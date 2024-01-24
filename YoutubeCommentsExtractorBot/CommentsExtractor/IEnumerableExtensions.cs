using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentsExtractor
{
    internal static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }

        public static T GetRandElement<T>(this T[] array)
        {
            Random random = new Random();
            int index = random.Next(0, array.Length - 1);

            return array[index];
        }
    }
}
