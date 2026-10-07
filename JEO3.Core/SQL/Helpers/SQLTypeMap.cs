using System.Data;

namespace JEO3.Core
{
    public sealed class SQLTypeMap
    {
        /// <summary>
        /// List of SQL Type Codes for Model Hydration. Note: They do not affect the retrieval of a DataTable from a SQL query, only DataTable --> Hydration of a model of Type T
        /// For example, if model <T> has a property of type int, then this list will be used to determine that the TypeCode for int is Int32, 
        /// which will then be used in the SetPropertyValue function to set the value of the property correctly. UDT's: Flag, HierarchyId, Geography, Geometry
        /// 
        /// DO NOT REMOVE
        /// </summary>
        public static readonly List<KeyValuePair<Type, TypeCode>> TypeCodes =
        [
                new KeyValuePair<Type, TypeCode>(typeof(Boolean), TypeCode.Boolean),
                new KeyValuePair<Type, TypeCode>(typeof(bool), TypeCode.Boolean),
                new KeyValuePair<Type, TypeCode>(typeof(byte), TypeCode.Byte),
                new KeyValuePair<Type, TypeCode>(typeof(char), TypeCode.Char),
                new KeyValuePair<Type, TypeCode>(typeof(Char), TypeCode.Char),
                new KeyValuePair<Type, TypeCode>(typeof(DateTime), TypeCode.DateTime),
                new KeyValuePair<Type, TypeCode>(typeof(decimal), TypeCode.Decimal),
                new KeyValuePair<Type, TypeCode>(typeof(Decimal), TypeCode.Decimal),
                new KeyValuePair<Type, TypeCode>(typeof(double), TypeCode.Double),
                new KeyValuePair<Type, TypeCode>(typeof(Double), TypeCode.Double),
                new KeyValuePair<Type, TypeCode>(typeof(short), TypeCode.Int16),
                new KeyValuePair<Type, TypeCode>(typeof(int), TypeCode.Int32),
                new KeyValuePair<Type, TypeCode>(typeof(long), TypeCode.Int64),
                new KeyValuePair<Type, TypeCode>(typeof(float), TypeCode.Single),
                new KeyValuePair<Type, TypeCode>(typeof(string), TypeCode.String),
                new KeyValuePair<Type, TypeCode>(typeof(String), TypeCode.String),
                new KeyValuePair<Type, TypeCode>(typeof(ushort), TypeCode.UInt16),
                new KeyValuePair<Type, TypeCode>(typeof(uint), TypeCode.UInt32),
                new KeyValuePair<Type, TypeCode>(typeof(Int16), TypeCode.Int16),
                new KeyValuePair<Type, TypeCode>(typeof(Int32), TypeCode.Int32),
                new KeyValuePair<Type, TypeCode>(typeof(Int64), TypeCode.Int64),
                new KeyValuePair<Type, TypeCode>(typeof(UInt16), TypeCode.UInt16),
                new KeyValuePair<Type, TypeCode>(typeof(UInt32), TypeCode.UInt32),
                new KeyValuePair<Type, TypeCode>(typeof(UInt64), TypeCode.UInt64)
            ];

        // Map: C# Type -> SqlDbType
        internal static readonly Dictionary<Type, SqlDbType> TypeToSqlMap = new()
        {
            { typeof(bool), SqlDbType.Bit },
            { typeof(byte), SqlDbType.TinyInt },
            { typeof(short), SqlDbType.SmallInt },
            { typeof(int), SqlDbType.Int },
            { typeof(long), SqlDbType.BigInt },
            { typeof(float), SqlDbType.Real },
            { typeof(double), SqlDbType.Float },
            { typeof(decimal), SqlDbType.Decimal },
            { typeof(string), SqlDbType.NVarChar },
            { typeof(char), SqlDbType.NChar },
            { typeof(Guid), SqlDbType.UniqueIdentifier },
            { typeof(DateTime), SqlDbType.DateTime2 },
            { typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
            { typeof(TimeSpan), SqlDbType.Time },
            { typeof(byte[]), SqlDbType.VarBinary }
        };

        // Map: SqlDbType -> C# Type
        internal static readonly Dictionary<SqlDbType, Type> SqlToTypeMap = new()
        {
            { SqlDbType.Bit, typeof(bool) },
            { SqlDbType.TinyInt, typeof(byte) },
            { SqlDbType.SmallInt, typeof(short) },
            { SqlDbType.Int, typeof(int) },
            { SqlDbType.BigInt, typeof(long) },
            { SqlDbType.Real, typeof(float) },
            { SqlDbType.Float, typeof(double) },
            { SqlDbType.Decimal, typeof(decimal) },
            { SqlDbType.Money, typeof(decimal) },
            { SqlDbType.SmallMoney, typeof(decimal) },
            { SqlDbType.VarChar, typeof(string) },
            { SqlDbType.NVarChar, typeof(string) },
            { SqlDbType.Text, typeof(string) },
            { SqlDbType.NText, typeof(string) },
            { SqlDbType.Char, typeof(string) },
            { SqlDbType.NChar, typeof(string) },
            { SqlDbType.UniqueIdentifier, typeof(Guid) },
            { SqlDbType.Date, typeof(DateTime) },
            { SqlDbType.DateTime, typeof(DateTime) },
            { SqlDbType.DateTime2, typeof(DateTime) },
            { SqlDbType.SmallDateTime, typeof(DateTime) },
            { SqlDbType.DateTimeOffset, typeof(DateTimeOffset) },
            { SqlDbType.Time, typeof(TimeSpan) },
            { SqlDbType.Binary, typeof(byte[]) },
            { SqlDbType.VarBinary, typeof(byte[]) },
            { SqlDbType.Image, typeof(byte[]) },
            { SqlDbType.Timestamp, typeof(Byte[])}
        };
    }
}
