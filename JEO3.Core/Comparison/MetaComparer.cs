namespace JEO3.Core
{
    public sealed class MetaComparer<T> : IEqualityComparer<Meta<T>>, IComparer<Meta<T>>
    {
        public bool Equals(Meta<T>? x, Meta<T>? y) => x?.Display == y?.Display;
        public int GetHashCode(Meta<T> obj) => obj.Display?.GetHashCode() ?? 0;
        public int Compare(Meta<T>? x, Meta<T>? y) => string.Compare(x?.Display, y?.Display, StringComparison.OrdinalIgnoreCase);
    }
}
