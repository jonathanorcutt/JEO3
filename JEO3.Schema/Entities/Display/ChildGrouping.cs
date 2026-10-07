using System.Collections;

namespace JEO3.Schema
{
    // ChildGrouping.cs — JEO3.Schema
    public interface IChildGrouping
    {
        string Name { get; }
        IEnumerable Children { get; }
    }

    public sealed class ChildGrouping<T> : IChildGrouping where T : IObject
    {
        public string Name { get; internal init; } = typeof(T).Name;
        public IEnumerable Children { get; internal init; }
        IEnumerable IChildGrouping.Children => Children;
    }
}