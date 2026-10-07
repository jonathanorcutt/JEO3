using System.Data;
using System.Reflection;
using JEO3.Core;

namespace JEO3.Extensions
{
    internal static class DataRowExtensions
    {
        internal static T ToInstance<T>(this DataRow dr, List<string> columnNames, List<PropertyInfo> properties)
        {
            // Create New Instance Of Type T
            T item = (T)Activator.CreateInstance(typeof(T));

            // Loop Type Properties
            foreach (var property in properties)
            {
                // Find matching column name safely
                string columnName = columnNames.FirstOrDefault(name => string.Equals(name, property.Name, StringComparison.OrdinalIgnoreCase));

                if (columnName == null)
                {
                    continue;
                }

                // Check if the DataRow contains a valid database value
                var rawValue = dr[columnName];
                if (rawValue == null || rawValue == DBNull.Value)
                {
                    continue;
                }

                var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                try
                {
                    // Check if the type can be assigned directly (Works for hierarchyid, geography, geometry, etc.)
                    if (property.PropertyType.IsAssignableFrom(rawValue.GetType()) || underlyingType.IsAssignableFrom(rawValue.GetType()))
                    {
                        SetPropertyValue(item, property, rawValue);
                        continue;
                    }

                    // Fallback to your custom SQLTypeMap dictionary for standard primitive type casting
                    var matchingPair = SQLTypeMap.TypeCodes.FirstOrDefault(kvp => kvp.Key == underlyingType);
                    if (matchingPair.Key != null)
                    {
                        // Convert type safely using your mapping logic
                        var convertedValue = Convert.ChangeType(rawValue, matchingPair.Value);
                        SetPropertyValue(item, property, convertedValue);
                    }
                }
                catch (Exception ex)
                {
                    // Consider logging the error here to catch missing assembly references for SQL types
                }
            }

            return item;
        }
        private static bool SetPropertyValue(object item, PropertyInfo property, object? value)
        {
            try
            {
                if (value == null || value == DBNull.Value)
                    return true;

                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                object converted;

                if (targetType == typeof(string))
                {
                    converted = value.ToString()!;
                }
                else if (targetType == typeof(bool))
                {
                    if (value is bool b)
                    {
                        converted = b;
                    }
                    else
                    {
                        var s = value.ToString()?.Trim().ToUpperInvariant();

                        converted = s switch
                        {
                            "1" => true,
                            "Y" => true,
                            "YES" => true,
                            "TRUE" => true,
                            _ => false
                        };
                    }
                }
                // --- ADDED ONLY THIS CHECK FOR THE SQL COMPLEX TYPES ---
                else if (value.GetType() == targetType || targetType.IsAssignableFrom(value.GetType()))
                {
                    converted = value;
                }
                else
                {
                    converted = targetType == typeof(TimeSpan)
                        ? value is TimeSpan ts ? ts : TimeSpan.Parse(value.ToString()!)
                        : targetType.IsEnum
                        ? Enum.ToObject(targetType, value)
                        : value.GetType() == targetType ? value : Convert.ChangeType(value, targetType);
                }

                var setter = property.GetSetMethod(true);
                var setter2 = property.GetSetMethod(false);

                if (setter != null)
                {
                    setter.Invoke(item, new[] { converted });
                }
                else if (setter2 != null)
                {
                    setter2.Invoke(item, new[] { converted });
                }
                else
                {
                    if (property.Name != "TablePath" && property.Name != "Path")
                    {
                        bool f = true;
                    }
                    //property.SetValue(item, converted);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
