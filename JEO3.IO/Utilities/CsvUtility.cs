namespace JEO3.IO
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public static class CsvUtility
    {
        private class ColumnMapper
        {
            public string Header { get; set; } = string.Empty;
            public Func<object?, string> GetValue { get; set; } = _ => string.Empty;
        }

        /// <summary>
        /// Flattens an IEnumerable of T into a nested CSV format.
        /// </summary>
        public static string ToCsv<T>(IEnumerable<T> data, int maxDepth = 3)
        {
            if (data == null || !data.Any()) return string.Empty;

            // Build the schema and delegates once
            var mappers = GetMappers(typeof(T), string.Empty, 0, maxDepth);
            var sb = new StringBuilder();

            // Write Headers
            sb.AppendLine(string.Join(",", mappers.Select(m => EscapeCsv(m.Header))));

            // Write Rows
            foreach (var item in data)
            {
                var rowValues = mappers.Select(m => EscapeCsv(m.GetValue(item)));
                sb.AppendLine(string.Join(",", rowValues));
            }

            return sb.ToString();
        }

        private static List<ColumnMapper> GetMappers(Type type, string prefix, int depth, int maxDepth)
        {
            var mappers = new List<ColumnMapper>();

            // Protects against infinite loops in cyclic graphs
            if (depth > maxDepth) return mappers;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var propType = prop.PropertyType;
                var colName = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";

                if (IsSimpleType(propType))
                {
                    // Leaf Node: Primitive or String
                    mappers.Add(new ColumnMapper
                    {
                        Header = colName,
                        GetValue = obj =>
                        {
                            if (obj == null) return string.Empty;
                            var val = prop.GetValue(obj);
                            return val?.ToString() ?? string.Empty;
                        }
                    });
                }
                else if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
                {
                    // 1-to-N Collection: Flatten into a pipe-delimited string
                    mappers.Add(new ColumnMapper
                    {
                        Header = colName,
                        GetValue = obj =>
                        {
                            if (obj == null) return string.Empty;
                            var collection = prop.GetValue(obj) as IEnumerable;
                            if (collection == null) return string.Empty;

                            var elements = new List<string>();
                            foreach (var element in collection)
                            {
                                elements.Add(element?.ToString() ?? string.Empty);
                            }
                            return string.Join("|", elements);
                        }
                    });
                }
                else
                {
                    // 1-to-1 Nested Object: Recurse and build composite headers
                    var nestedMappers = GetMappers(propType, colName, depth + 1, maxDepth);

                    foreach (var nested in nestedMappers)
                    {
                        var originalGetValue = nested.GetValue;
                        nested.GetValue = obj =>
                        {
                            if (obj == null) return string.Empty;
                            var nestedObj = prop.GetValue(obj);

                            // Pass the nested object down the chain (even if it's null, 
                            // so the underlying primitives return empty strings to preserve column alignment)
                            return originalGetValue(nestedObj);
                        };
                        mappers.Add(nested);
                    }
                }
            }

            return mappers;
        }

        private static bool IsSimpleType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType.IsPrimitive ||
                   underlyingType.IsEnum ||
                   underlyingType == typeof(string) ||
                   underlyingType == typeof(decimal) ||
                   underlyingType == typeof(DateTime) ||
                   underlyingType == typeof(Guid);
        }

        private static string EscapeCsv(string? value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r')
                ? $"\"{value.Replace("\"", "\"\"")}\""
                : value;
        }
    }
}
