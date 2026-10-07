using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace JEO3.Core
{
    public sealed class Meta<T> : IEqualityComparer<T>
    {
        public string Display { get; }
        public string Safe { get; }
        public T Value { get; }

        public Meta(string display, T value)
        {
            Display = display;
            Safe = Regex.Replace(display, @"[^a-zA-Z0-9_]", "_");
            Value = value;
        }

        public bool Equals(T? x, T? y)
        {
            return x?.ToString() == y?.ToString();
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            return obj?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Display;
        }
    }
}
