using System.Reflection;

namespace JEO3.Extensions
{
    public static class GenericExtensions
    {
        public static List<List<T>> BreakIntoSmallerGroups<T>(this List<T> list, int groupSize)
        {
            // Dont Mess Up The Original List - ByRef
            var tmpList = list.Select(v => v).ToList();

            List<List<T>> groups = [];
            while (tmpList.Count >= groupSize)
            {
                groups.Add(tmpList.Take(groupSize).ToList());
                tmpList.RemoveRange(0, groupSize);
            }

            if (tmpList.Count > 0)
            {
                groups.Add(tmpList.Take(tmpList.Count).ToList());
            }

            return groups;
        }

        public static void TrimStringValues<T>(this IEnumerable<T> list)
        {
            foreach (var item in list)
            {
                TrimStringValues(item);
            }
        }

        public static void TrimStringValues<T>(this T obj)
        {
            // Validation
            if (obj == null)
            {
                return;
            }

            // Get Properties
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => typeof(string).IsAssignableFrom(prop.PropertyType) && prop.SetMethod != null && prop.SetMethod.IsPublic == true).ToList();

            // Loop Properties
            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                value = value == null ? string.Empty : value.ToString().Trim();

                prop.SetValue(obj, value);
            }
        }
    }
}
