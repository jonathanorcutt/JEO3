using JEO3.Generation.Models;
using JEO3.Schema;

namespace JEO3.Code
{
    public sealed class IndexCodeStringGenerator
    {
        public static string GetDatabaseMissingIndexesSql(DatabaseContext context, QueryGenerationResult result, bool missingOnly = true)
        {
            const string BR = "\r\n";
            string sql = "";
            foreach (var tbl in context.Tables.OrderBy(v => v.DatabaseName).ThenBy(v => v.SchemaName).ThenBy(v => v.Name))
            {
                if (tbl.MissingIndexes.Any() == false && missingOnly) { continue; }
                sql += GetTableMissingIndexesSql(context, tbl, result);
            }
            return sql;
        }
        public static string GetTableMissingIndexesSql(DatabaseContext context, ITable tbl, QueryGenerationResult result)
        {
            const string BR = "\r\n";
            string tblSql = $"{BR}{BR}-- TABLE: {tbl.SchemaName}.{tbl.Name}";
            var gen = new TableCodeGenerator(context, tbl, tbl.DatabaseName, false);
            tblSql += BR + gen.GenerateTableAnnotationOnly(result, "--   ");
            tblSql += $"{BR}-- EXISTING INDEXES - {tbl.Indexes.Count}";
            foreach (var idx in tbl.Indexes)
            {
                tblSql += GetIndexSql(idx);
            }
            tblSql += $"{BR}-- SUGGESTED/MISSING INDEXES - {tbl.MissingIndexes.Count}";
            foreach (var idx in tbl.MissingIndexes)
            {
                tblSql += GetMissingIndexSql(idx);
            }
            return tblSql;
        }
        public static string GetIndexSql(IIndex idx)
        {
            const string BR = "\r\n";
            return $"{BR}--   Name: {idx.Name}|Type:{idx.IndexType}|Frag:{idx.FragmentationPercentage}|Key Cols:{string.Join(", ", idx.KeyColumns.Select(v => $"({v.DataType}{(v.IsNullable ? "NULL " : "")}) {v.Name}"))}|Include Columns:{String.Join(", ", idx.IncludedColumns.Select(v => $"({v.DataType}{(v.IsNullable ? "NULL " : "")}) {v.Name}"))}|PK:{idx.IsPrimaryKey}|Unique:{idx.IsUnique}|UniqueConstraint:{idx.IsUniqueConstraint}|{idx.IsDisabled}|Page Count:{idx.PageCount}|Is Covering:{idx.IsCovering}|Clustered {idx.IsClustered}|Filter: {idx.FilterDefinition}";
        }
        public static string GetMissingIndexSql(IMissingIndex idx)
        {
            const string BR = "\r\n";
            var tblSql = $"{BR}--   IndexGroupHandle: {idx.IndexGroupHandle}|IndexHandle:{idx.IndexHandle}|ImprovementMeasure:{idx.ImprovementMeasure}"
                + $"|AvgTotalUserCost:{idx.AvgTotalUserCost}|AvgUserImpact:{idx.AvgUserImpact}|Score:{idx.Score}|Inequality:{idx.InequalityColumnsDisplayString}"
                + $"|Equality:{idx.EqualityColumnsDisplayString}|Include:{idx.IncludedColumnsDisplayString}|Key Columns:{idx.SuggestedKeyColumnsDisplayString}|UserReads:{idx.UserReads}|UserSeeks:{idx.UserSeeks}|UserScans{idx.UserScans}";
            tblSql += $"{BR}{idx.IndexCreationScript}";
            return tblSql;
        }
    }
}