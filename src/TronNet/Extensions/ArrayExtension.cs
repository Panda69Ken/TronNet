using System.Linq;

namespace TronNet
{
    public static class ArrayExtension
    {
        public static T[] SubArray<T>(this T[] array, int offset, int length)
        {
            return array.Skip(offset)
                        .Take(length)
                        .ToArray();
        }
    }
}
