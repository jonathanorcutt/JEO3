using System.Data;

namespace JEO3.Core
{
    public static class SQLTypeMapper
    {
        public static SqlDbType ToSqlDbType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            // Unwrap Nullable<T> if present (e.g., int? -> int)
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return SQLTypeMap.TypeToSqlMap.TryGetValue(underlyingType, out SqlDbType sqlDbType)
                ? sqlDbType
                : throw new ArgumentException($"No SqlDbType mapping found for C# type: {type.FullName}");
        }
        public static Type ToCsharpType(SqlDbType sqlDbType)
        {
            return SQLTypeMap.SqlToTypeMap.TryGetValue(sqlDbType, out Type? csharpType)
                ? csharpType
                : throw new ArgumentException($"No C# type mapping found for SqlDbType: {sqlDbType}");
        }
        public static string SqlTypeToClr(string dataType, bool nullable)
        {
            string type = dataType!.Split('(')[0].ToLower() switch
            {
                "bigint" => "long",
                "int" => "int",
                "smallint" => "short",
                "tinyint" => "byte",
                "bit" => "bool",

                "decimal" => "decimal",
                "numeric" => "decimal",
                "money" => "decimal",
                "smallmoney" => "decimal",

                "float" => "double",
                "real" => "float",

                "date" => "DateTime",
                "datetime" => "DateTime",
                "datetime2" => "DateTime",
                "smalldatetime" => "DateTime",
                "time" => "TimeSpan",

                "uniqueidentifier" => "Guid",

                "binary" => "byte[]",
                "varbinary" => "byte[]",
                "image" => "byte[]",

                "char" => "string",
                "varchar" => "string",
                "nchar" => "string",
                "nvarchar" => "string",
                "text" => "string",
                "ntext" => "string",
                "xml" => "string",

                _ => "object"
            };

            return type is "string" or "byte[]" or "object" ? type : nullable ? $"{type}?" : type;
        }
    }
}
