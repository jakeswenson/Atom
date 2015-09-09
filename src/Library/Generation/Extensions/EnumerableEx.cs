using System.Collections.Generic;

namespace Atom.Generation.Extensions
{
    public static class EnumerableEx
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> items)
        {
            return new HashSet<T>(items);
        }

        public static Stack<T> ToStack<T>(this IEnumerable<T> items)
        {
            return new Stack<T>(items);
        }
    }
}
