using System.Collections.Generic;

namespace LazyReadOnlyList
{
    public static class Extensions
    {
        public static LazyReadOnlyList<T> ToLazyReadOnlyList<T>(this IEnumerable<T> source)
        {
            return new LazyReadOnlyList<T>(source);
        }
    }
}
