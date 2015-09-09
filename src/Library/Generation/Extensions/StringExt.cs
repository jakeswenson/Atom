using System;
using System.Collections.Generic;
using System.Linq;

namespace Atom.Generation.Extensions
{
    internal static class StringExt
    {
        public static string WithTab(this string src, int n = 1)
        {
            const int tabSize = 4;
            return new string(' ', tabSize * n) + src;
        }

        public static string AppendTab(this string src, int n = 1)
        {
            const int tabSize = 4;
            return src + (new string(' ', tabSize * n));
        }

        public static string IndentAllLines(this string src, int byTabs, bool ignoreFirst = false)
        {
            Func<string, int, string> splitter = (entry, n) => n == 0 && ignoreFirst ? entry : entry.WithTab(byTabs);

            var splits = src.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                            .Select(splitter);

            var result = string.Join(Environment.NewLine, splits);

            return result;
        }

        public static IEnumerable<string> IndentAllLines(this IEnumerable<string> src, int byTabs)
        {
            foreach (var item in src)
            {
                yield return item.IndentAllLines(byTabs);
            }
        }

        public static string ToTitleCase(string name)
        {
            return name.First()
                       .ToString()
                       .ToUpper() + name.Substring(1);
        }

        public static string ToCamelCase(string name)
        {
            return name.First()
                       .ToString()
                       .ToLower() + name.Substring(1);
        }

        public static IEnumerable<string> IndentAllLines(this IEnumerable<string> src, int byTabs, bool ignoreFirst)
        {
            var count = 0;
            foreach (var item in src)
            {
                if (count == 0 && ignoreFirst)
                {
                    yield return item;
                }
                else
                {
                    yield return item.IndentAllLines(byTabs);
                }

                count++;
            }
        }
    }
}
