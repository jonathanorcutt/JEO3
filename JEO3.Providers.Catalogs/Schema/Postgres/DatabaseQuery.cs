using System;
using System.Collections.Generic;
using System.Text;

namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        public const string DatabaseQuery = @"
SELECT
    d.oid AS ObjectId,
    d.datname AS Name,
    d.datname AS DatabaseName,
    CAST(current_setting('server_version_num') AS integer) AS CompatibilityLevel,
    TRUE AS IsReadCommitted,
    NULL::timestamp AS CreateDate,
    d.datcollate AS CollationName
FROM pg_database d
WHERE d.datname = current_database();
";
    }
}
