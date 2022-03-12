using System;
using System.Collections.Generic;
using System.Linq;

namespace Helper.Core.Utils
{
    public static class Extensions
    {
        public static T[] Remove<T>(this IEnumerable<T> source, T value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (value == null) throw new ArgumentNullException(nameof(value));

            var list = source.ToList();
            list.Remove(value);
            return list.ToArray();
        }
    }
}
