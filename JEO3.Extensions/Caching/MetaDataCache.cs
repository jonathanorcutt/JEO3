using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace JEO3.Extensions
{
    public static class MetaDataCache
    {
        // 🌟 Cache A: Static cache for raw public property arrays per Type context
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        // 🌟 Cache B: Compiled Expression Tree Getters for hardcoded native execution speed
        private static readonly ConcurrentDictionary<(Type Type, string PropName), Func<object, object>> _getterCache = new();

        /// <summary>
        /// Gets all public instance properties for any type, fully cached.
        /// </summary>
        public static PropertyInfo[] GetProperties(Type type)
        {
            if (type == null) return Array.Empty<PropertyInfo>();
            return _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        }

        /// <summary>
        /// Gets all public and non-public instance properties for internal mapping logic, fully cached.
        /// </summary>
        public static PropertyInfo[] GetAllProperties(Type type)
        {
            if (type == null) return Array.Empty<PropertyInfo>();
            return _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
        }

        /// <summary>
        /// Compiles a property getter into a high-speed execution delegate on the fly.
        /// </summary>
        public static object GetPropertyValue(object instance, PropertyInfo property)
        {
            if (instance == null || property == null) return null!;

            var key = (instance.GetType(), property.Name);

            var getter = _getterCache.GetOrAdd(key, k =>
            {
                var parameter = Expression.Parameter(typeof(object), "obj");
                var castTarget = Expression.Convert(parameter, k.Type);
                var propertyAccess = Expression.Property(castTarget, property);
                var castResult = Expression.Convert(propertyAccess, typeof(object));

                return Expression.Lambda<Func<object, object>>(castResult, parameter).Compile();
            });

            return getter(instance);
        }
    }
}
