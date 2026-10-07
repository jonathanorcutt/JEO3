namespace JEO3.Providers.Catalogs.Extensions
{
    public static class SqlDateTimeExtensions
    {
        /// <summary>
        /// Wraps the DateTime into a deterministic, unambiguous SQL Server CONVERT statement.
        /// Prevents regional configuration style interpretation explosions.
        /// </summary>
        public static string ToSqlDatetime2(this DateTime dateTime)
        {
            // Forces standard ISO 8601 format: yyyy-MM-ddTHH:mm:ss.fff
            // Format code 126 guarantees standard ISO translation inside SQL Server
            return $"CONVERT(DATETIME2(3), '{dateTime:yyyy-MM-ddTHH:mm:ss.fff}', 126)";
        }
    }
}
