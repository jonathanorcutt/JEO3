using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace JEO3.Providers.Extensions
{
    internal static class EntityScanner
    {
        internal static EntityDescriptor Scan<T>(bool includeCollections = false) where T : class
        {
            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<JeoTable>()
                ?? throw new InvalidOperationException($"Type {type.Name} is missing [{typeof(JeoTable).Name}].");
            var columns = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite && p.GetCustomAttributes().Any(v => v.GetType() == typeof(NotMappedAttribute)) == false 
                    && (includeCollections == false ? p.PropertyType.IsGenericType == false : true) 
                )
                .Select(p =>
                {
                    var colAttr = p.GetCustomAttribute<JeoKey>();
                    var isNullable = !p.PropertyType.IsValueType || Nullable.GetUnderlyingType(p.PropertyType) != null;

                    return new ColumnDescriptor
                    {
                        PropertyName = p.Name,
                        // Fallback to property name if column name isn't explicitly defined
                        ColumnName = string.IsNullOrWhiteSpace(colAttr?.Name) ? p.Name : colAttr.Name,
                        ClrType = p.PropertyType,
                        IsNullable = isNullable,
                        IsPrimaryKey = colAttr?.IsPrimaryKey ?? false,
                        IsForeignKey = colAttr?.IsForeignKey ?? false,
                        IsDbGenerated = colAttr?.IsDbGenerated ?? false,
                        Property = p
                    };
                }).ToList();

            var cols =  !columns.Any(c => c.IsPrimaryKey)
                ? throw new InvalidOperationException($"Type {type.Name} must define at least one Primary Key.")
                : new EntityDescriptor
                {
                    Schema = tableAttr.Schema,
                    TableName = tableAttr.Name,
                    Columns = columns
                };
            return cols;
        }
    }
}