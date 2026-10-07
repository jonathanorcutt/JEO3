using System.Text;
using JEO3.Schema;

namespace JEO3.Code
{
    public sealed class ProcedureCodeGenerator
    {
        public static string GenerateProcedures(ITable table)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GenerateInsert(table));
            sb.AppendLine();
            sb.AppendLine(GenerateUpdate(table));
            sb.AppendLine();
            sb.AppendLine(GenerateDelete(table));
            sb.AppendLine();
            sb.AppendLine(GenerateGetById(table));
            sb.AppendLine();
            sb.AppendLine(GenerateGetByIds(table));
            return sb.ToString();
        }

        private static string GenerateInsert(ITable table)
        {
            // Skip identity columns for the insert payload
            var insertable = table.Columns.Where(c => !c.IsIdentity).ToList();

            var spParams = string.Join(",\n    ", insertable.Select(c => $"@{c.Name} {c.DataType}"));
            var columns = string.Join(", ", insertable.Select(c => $"[{c.Name}]"));
            var values = string.Join(", ", insertable.Select(c => $"@{c.Name}"));

            return string.Format(SqlProcedureTemplates.SpInsert, table.SchemaName, table.Name, spParams, columns, values);
        }
        private static string GenerateUpdate(ITable table)
        {
            var pks = table.Columns.Where(c => c.IsPrimaryKey).ToList();
            var updateable = table.Columns.Where(c => !c.IsIdentity && !c.IsPrimaryKey).ToList();

            // Needs parameters for EVERYTHING (keys to find the row, updateable columns to change it)
            var spParams = string.Join(",\n    ", table.Columns.Where(c => !c.IsIdentity || c.IsPrimaryKey)
                                                             .Select(c => $"@{c.Name} {c.DataType}"));

            var setExpressions = string.Join(",\n    ", updateable.Select(c => $"[{c.Name}] = @{c.Name}"));
            var whereClause = string.Join(" AND ", pks.Select(c => $"[{c.Name}] = @{c.Name}"));

            return string.Format(SqlProcedureTemplates.SpUpdate, table.SchemaName, table.Name, spParams, setExpressions, whereClause);
        }
        private static string GenerateDelete(ITable table)
        {
            var pks = table.Columns.Where(c => c.IsPrimaryKey).ToList();

            var spParams = string.Join(",\n    ", pks.Select(c => $"@{c.Name} {c.DataType}"));
            var whereClause = string.Join(" AND ", pks.Select(c => $"[{c.Name}] = @{c.Name}"));

            return string.Format(SqlProcedureTemplates.SpDelete, table.SchemaName, table.Name, spParams, whereClause);
        }
        private static string GenerateGetById(ITable table)
        {
            var pks = table.Columns.Where(c => c.IsPrimaryKey).ToList();

            var spParams = string.Join(",\n    ", pks.Select(c => $"@{c.Name} {c.DataType}"));
            var selectColumns = string.Join(", ", table.Columns.Select(c => $"[{c.Name}]"));
            var whereClause = string.Join(" AND ", pks.Select(c => $"[{c.Name}] = @{c.Name}"));

            return string.Format(SqlProcedureTemplates.SpGetById, table.SchemaName, table.Name, spParams, selectColumns, whereClause);
        }
        private static string GenerateGetByIds(ITable table)
        {
            var pk = table.Columns.FirstOrDefault(c => c.IsPrimaryKey);
            if (pk == null) return $"-- Cannot generate GetByIds for {table.Name}: No Primary Key defined.";

            // Using modern SQL Server STRING_SPLIT for the CSV approach
            var spParams = "@Ids VARCHAR(MAX)";
            var selectColumns = string.Join(", ", table.Columns.Select(c => $"X.[{c.Name}]"));

            // Casts the string_split value to match the exact PK data type

            return string.Format(SqlProcedureTemplates.SpGetByIds, table.SchemaName, table.Name, pk.Name, pk.DataType, selectColumns);
        }
    }
}