using System.Collections.Generic;
using System.Linq;

namespace Daihenka.ShaderStripper
{
    internal static class CollectionUtility
    {
        public static bool HasSameElements<T>(this IEnumerable<T> list, IEnumerable<T> other)
        {
            if (list == null || other == null) return false;

            var listA = list as T[] ?? list.ToArray();
            var listB = other as T[] ?? other.ToArray();
            return listA.Length == listB.Length &&
                   (listA.Length == 0 || listA.Intersect(listB).Count() == listA.Length);
        }

        public static bool HasSameElements<T>(this ICollection<T> list, ICollection<T> other)
        {
            return list != null && other != null &&
                   list.Count == other.Count &&
                   (list.Count == 0 || list.Intersect(other).Count() == list.Count);
        }
    }
}