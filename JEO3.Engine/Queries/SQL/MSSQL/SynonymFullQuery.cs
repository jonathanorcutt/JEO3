using JEO3.Schema;

namespace JEO3.Engine.Queries.SQL.MSSQL
{
    internal static partial class MSSqlQueries
    {
        internal const string SynonymFullQuery = @"
SELECT 
    object_id AS " + nameof(ISynonym.Id) + @",
    schema_id AS " + nameof(ISynonym.SchemaId) + @",
    name AS [" + nameof(ISynonym.Name) + @"],
    base_object_name AS " + nameof(ISynonym.BaseObjectName) + @",
    principal_id AS " + nameof(ISynonym.PrincipalId) + @",
    parent_object_id AS " + nameof(ISynonym.ParentObjectId) + @",
    type AS [" + nameof(ISynonym.Type) + @"],
    type_desc AS " + nameof(ISynonym.TypeDesc) + @",
    create_date AS " + nameof(ISynonym.CreateDate) + @",
    modify_date AS " + nameof(ISynonym.ModifyDate) + @",
    is_ms_shipped AS " + nameof(ISynonym.IsMsShipped) + @",
    is_published AS " + nameof(ISynonym.IsPublished) + @",
    is_schema_published AS " + nameof(ISynonym.IsSchemaPublished) + @"
FROM sys.synonyms WITH(NOLOCK);";
    }
}
