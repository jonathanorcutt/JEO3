namespace JEO3.Core
{
    public sealed class MetadataScanner
    {
        #region Properties
        private Dictionary<string, string> _formattedProperties { get; } = new();
        private bool _hasScannedTypes = false;
        #endregion

        #region Initialization
        public MetadataScanner()
        {
        }
        #endregion

        #region Discovery
        private void ScanRootType<T>(Type rootType, T attributeType) where T : Attribute
        {
            var visitedTypes = new HashSet<Type>();
            DiscoverPropertiesRecursive(rootType, attributeType, visitedTypes);
        }
        private void DiscoverPropertiesRecursive<T>(Type type, T attributeType, HashSet<Type> visited) where T : Attribute
        {
            if (type == null)
            {
                return;
            }

            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType.IsPrimitive || underlyingType == typeof(string) || underlyingType == typeof(DateTime))
            {
                return;
            }
            if (underlyingType.Name != null && underlyingType.Namespace.StartsWith("System"))
            {
                return;
            }
            if (!visited.Add(type))
            {
                return;
            }

            foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var attr = prop.GetCustomAttributes(true);
                if (attr != null)
                {
                    string uniqueKey = $"{type.FullName}.{prop.Name}";
                    //FormattedProperties[uniqueKey] = attr.Format;
                }
                if (typeof(IEnumerable<>).IsAssignableFrom(prop.PropertyType))
                {
                    if (prop.PropertyType.IsGenericType)
                    {
                        foreach (var genericArg in prop.PropertyType.GetGenericArguments())
                        {
                            DiscoverPropertiesRecursive(genericArg, attributeType, visited);
                        }
                    }
                }
                else if ((prop.PropertyType.IsClass && prop.PropertyType != typeof(string)) ||
                    (prop.PropertyType.IsValueType && !prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(DateTime) && !prop.PropertyType.IsEnum))
                {
                    DiscoverPropertiesRecursive(prop.PropertyType, attributeType, visited);
                }
            }
        }
        #endregion
    }
}
