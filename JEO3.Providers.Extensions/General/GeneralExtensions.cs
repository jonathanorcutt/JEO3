using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace JEO3.Providers.Extensions
{
    internal class GeneralExtensions
    {
        internal static List<string> GetUniqueColumnNames(IDataReader reader)
        {
            var result = new List<string>();
            var occurrences = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);

                if (occurrences.TryGetValue(columnName, out int count))
                {
                    count++;
                    occurrences[columnName] = count;
                    columnName = $"{columnName}_{count}";
                }
                else
                {
                    occurrences[columnName] = 0;
                }

                result.Add(columnName);
            }

            return result;
        }
        internal static string GetColumnName<T, TValue>(Expression<Func<T, TValue>> selector)
        {
            var body = selector.Body is UnaryExpression u ? u.Operand : selector.Body;
            if (body is not MemberExpression member || member.Member is not PropertyInfo prop)
                throw new ArgumentException("Selector expression must point directly to a valid property.", nameof(selector));
            var name = prop.GetCustomAttribute<JeoKey>()?.Name; return string.IsNullOrEmpty(name) ? prop.Name : name;
        }
    }
}
