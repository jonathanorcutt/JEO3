using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using JEO3.Core;

namespace JEO3.Extensions
{
    public static class DataTableExtensions
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, string>> _caseInsensitiveMappingCache = new();

        private static string CleanStringSignature(string v)
        {
            if (string.IsNullOrEmpty(v)) return string.Empty;
            return v.Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty).ToLower();
        }

        #region Extracted Datatable Operations

        // 🌟 NO MORE CONSTRAINTS: Handles structs, records, and raw value tracking types flawlessly!
        public static DataTable ToDataTable<T>(this IEnumerable<T> items)
        {
            Type targetType = typeof(T);
            DataTable dt = new DataTable(targetType.Name);

            // 🌟 THE FINAL FIX: Resolving pure generic properties array from cache!
            PropertyInfo[] properties = MetaDataCache.GetProperties(targetType);

            foreach (PropertyInfo prop in properties)
            {
                Type propType = prop.PropertyType;
                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propType = Nullable.GetUnderlyingType(propType);
                }
                dt.Columns.Add(prop.Name, propType);
            }

            foreach (T item in items)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    // 🌟 COMPILED EXPRESSION TRICK: Expression compiled delegate getter value retrieval pass
                    values[i] = MetaDataCache.GetPropertyValue(item!, properties[i]) ?? DBNull.Value;
                }
                dt.Rows.Add(values);
            }
            return dt;
        }

        public static DataTable ToDataTableDisplay<T>(this IEnumerable<T> items)
        {
            Type targetType = typeof(T);
            DataTable dt = new DataTable(targetType.Name);

            // 🌟 THE FIX: Call the pure generic property cache, completely free of any table attributes!
            PropertyInfo[] properties = MetaDataCache.GetProperties(targetType);

            foreach (PropertyInfo prop in properties)
            {
                Type propType = prop.PropertyType;

                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    propType = Nullable.GetUnderlyingType(propType);

                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Meta<>))
                {
                    dt.Columns.Add(prop.Name, typeof(string));
                }
                else
                {
                    dt.Columns.Add(prop.Name, propType);
                }
            }

            foreach (T item in items)
            {
                var row = dt.NewRow();
                foreach (PropertyInfo prop in properties)
                {
                    // 🌟 COMPILED EXPRESSION TRICK: High speed property access using your cached getter!
                    object val = MetaDataCache.GetPropertyValue(item!, prop);

                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Meta<>))
                    {
                        if (val == null)
                        {
                            row[prop.Name] = DBNull.Value;
                        }
                        else
                        {
                            dynamic meta = val;
                            row[prop.Name] = meta.Display ?? string.Empty;
                        }
                    }
                    else
                    {
                        row[prop.Name] = val ?? DBNull.Value;
                    }
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        public static List<T> ToList<T>(this DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return [];
            }

            // Create List Of Type T
            List<T> list = (List<T>)Activator.CreateInstance(typeof(List<T>))!;

            // 🌟 THE FIX: Call the pure generic property cache, completely free of any table attributes!
            var propertiesList = MetaDataCache.GetAllProperties(typeof(T)).ToList();

            // Get List Of Table Column Names
            List<string> columnNames = new List<string>(dt.Columns.Count);
            foreach (DataColumn column in dt.Columns)
            {
                columnNames.Add(column.ColumnName);
            }

            // Loop DataRows
            foreach (DataRow dr in dt.Rows)
            {
                // Create New Instance Of Type T
                T item = dr.ToInstance<T>(columnNames, propertiesList);

                // Add Instance To List
                list.Add(item);
            }

            return list;
        }

        public static List<T> ToListCaseInsensitive<T>(this DataTable dt) where T : new()
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return [];
            }

            Type typeT = typeof(T);

            // 🌟 THE FIX: Call the pure generic property cache, completely free of any table attributes!
            var properties = MetaDataCache.GetAllProperties(typeT);
            var columns = dt.Columns.Cast<DataColumn>().Select(v => v.ColumnName).ToList();

            var mapping = _caseInsensitiveMappingCache.GetOrAdd(typeT, _ =>
            {
                var innerMapping = new Dictionary<PropertyInfo, string>();
                foreach (var prop in properties)
                {
                    if (prop.GetSetMethod(nonPublic: true) == null) continue;

                    string propSig = CleanStringSignature(prop.Name);
                    string matchedCol = columns.FirstOrDefault(col => CleanStringSignature(col) == propSig);

                    if (matchedCol == null)
                    {
                        throw new FormatException($"Missing expected column for property: {prop.Name}");
                    }
                    innerMapping[prop] = matchedCol;
                }
                return innerMapping;
            });

            var list = new List<T>(dt.Rows.Count);
            foreach (DataRow row in dt.Rows)
            {
                var dto = new T();
                foreach (var kvp in mapping)
                {
                    var prop = kvp.Key;
                    var rawValue = row[kvp.Value]?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(rawValue)) continue;

                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    try
                    {
                        var convertedValue = targetType.IsEnum
                            ? Enum.Parse(targetType, rawValue, true)
                            : Convert.ChangeType(rawValue, targetType);

                        prop.SetValue(dto, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        throw new FormatException($"Failed to convert value '{rawValue}' for property '{prop.Name}'.", ex);
                    }
                }
                list.Add(dto);
            }
            return list;
        }

        #endregion

        #region Other

        public static string ToCSV(this DataTable dt, bool includeHeaders = true)
        {
            var csv = includeHeaders == false ? string.Empty : string.Join(",", dt.Columns.ToList().Select(v => v.ColumnName).ToList());

            var rows = dt.AsEnumerable().ToList().Select(row => string.Join(",", row.ItemArray.ToList()));
            csv += string.Join("\r\n", rows);

            return csv;
        }

        #endregion
    }
}