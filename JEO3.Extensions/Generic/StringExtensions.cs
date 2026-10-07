namespace JEO3.Extensions
{
    public static class StringExtensions
    {
        private static readonly string[] LineBreaks = { "\r\n", "\r", "\n" };
        public static string ReplaceInsensitive(this string s, List<char> find, string replaceWith)
        {
            return Replace(s, find.Select(v => v.ToString()).ToList(), replaceWith, StringComparison.OrdinalIgnoreCase);
        }

        public static string ReplaceInsensitive(this string s, char[] find, string replaceWith)
        {
            return Replace(s, find.Select(v => v.ToString()).ToList(), replaceWith, StringComparison.OrdinalIgnoreCase);
        }

        public static string ReplaceInsensitive(this string s, string[] find, string replaceWith)
        {
            return Replace(s, find.Select(v => v.ToString()).ToList(), replaceWith, StringComparison.OrdinalIgnoreCase);
        }

        public static string Replace(this string s, List<char> find, string replaceWith, StringComparison comparison = StringComparison.Ordinal)
        {
            return Replace(s, find.Select(v => v.ToString()).ToList(), replaceWith, comparison);
        }

        public static string Replace(this string s, char[] find, string replaceWith, StringComparison comparison = StringComparison.Ordinal)
        {
            return Replace(s, find.Select(v => v.ToString()).ToList(), replaceWith, comparison);
        }

        public static string Replace(this string s, string[] find, string replaceWith, StringComparison comparison = StringComparison.Ordinal)
        {
            return Replace(s, find.Select(v => v.ToString()).ToList(), replaceWith, comparison);
        }

        public static string Replace(this string s, List<string> find, string replaceWith, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(s) || find == null || find.Count == 0) return s;
            foreach (var f in find)
            {
                if (!string.IsNullOrEmpty(f))
                    s = s.Replace(f, replaceWith, comparison);
            }
            return s;
        }

        // Trim Start
        public static string TrimStart(this string s, string prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            return string.IsNullOrEmpty(s) || string.IsNullOrEmpty(prefix)
                ? s
                : s.StartsWith(prefix, comparison) ? s.Substring(prefix.Length) : s;
        }
        public static string TrimStart(this string s, IEnumerable<string> prefixes, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var match = prefixes.FirstOrDefault(p => !string.IsNullOrEmpty(p) && s.StartsWith(p, comparison));
            return match != null ? s.Substring(match.Length) : s;
        }

        // Trim End
        public static string TrimEnd(this string s, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            return string.IsNullOrEmpty(s) || string.IsNullOrEmpty(suffix)
                ? s
                : s.EndsWith(suffix, comparison) ? s.Substring(0, s.Length - suffix.Length) : s;
        }
        public static string TrimEnd(this string s, IEnumerable<string> suffixes, StringComparison comparison = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var match = suffixes.FirstOrDefault(suf => !string.IsNullOrEmpty(suf) && s.EndsWith(suf, comparison));
            return match != null ? s.Substring(0, s.Length - match.Length) : s;
        }

        // Ensure Prefix/Suffix
        public static string EnsurePrefix(this string s, string prefix, StringComparison comparison = StringComparison.Ordinal)
        {
            return string.IsNullOrEmpty(prefix) ? s : string.IsNullOrEmpty(s) ? prefix : s.StartsWith(prefix, comparison) ? s : prefix + s;
        }
        public static string EnsureSuffix(this string s, string suffix, StringComparison comparison = StringComparison.Ordinal)
        {
            return string.IsNullOrEmpty(suffix) ? s : string.IsNullOrEmpty(s) ? suffix : s.EndsWith(suffix, comparison) ? s : s + suffix;
        }

        // Split

        // Remove Empty
        public static List<string> Split(this string s, bool removeEmptyEntries = false)
        {
            var options = removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
            return s.Split(LineBreaks, options).ToList();
        }
        // Search
        public static List<string> Split(this string s, string find, StringComparison comparison = StringComparison.Ordinal, bool removeEmptyEntries = false, bool trimEntries = false)
        {
            var options = removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
            var lines = s.Split(LineBreaks, options).Where(l => l.Contains(find, comparison));
            return (trimEntries ? lines.Select(l => l.Trim()) : lines).ToList();
        }
        // Predicate
        public static List<string> Split(this string s, Func<string, bool> predicate, bool trimEntries = false)
        {
            var lines = s.Split(LineBreaks, StringSplitOptions.None).Where(predicate);
            return (trimEntries ? lines.Select(l => l.Trim()) : lines).ToList();
        }
    }
}
