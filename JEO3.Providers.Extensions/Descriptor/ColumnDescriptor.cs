using System.Reflection;

namespace JEO3.Providers.Extensions
{
    public sealed class ColumnDescriptor
    {
        public string PropertyName { get; init; } = string.Empty;
        public string ColumnName { get; init; } = string.Empty;
        public Type ClrType { get; init; } = default!;
        public bool IsPrimaryKey { get; init; }
        public bool IsForeignKey { get; set; }
        public bool IsDbGenerated { get; init; }
        public bool IsNullable { get; init; }

        // Retained for fast value extraction/hydration in execution layer
        public PropertyInfo Property { get; init; } = default!;
    }
}
