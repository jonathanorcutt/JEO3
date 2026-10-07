using JEO3.Providers.Catalogs.Catalogs;

namespace JEO3.Providers.Catalogs
{
    public sealed class PostgresSchemaQueryCatalog : BaseSchemaQueryCatalog
    {
        public override string DatabaseQuery => MSSQLQueries.DatabaseQuery;
        public override string SchemaQuery => MSSQLQueries.SchemaQuery;
        public override string TableQuery => PostgresQueries.TableQuery;
        public override string ColumnQuery => PostgresQueries.ColumnQuery;
        public override string RelationQuery => PostgresQueries.RelationQuery;
        public override string IndexQuery => PostgresQueries.IndexQuery;
        public override string StoredProcedureDefinitionQuery => PostgresQueries.StoredProcedureDefinitionQuery;
        public override string ViewDefinitionQuery => PostgresQueries.ViewDefinitionQuery;
        public override string FunctionDefinitionQuery => PostgresQueries.FunctionDefinitionQuery;
        public override string UserDefinedTypeDefinitionQuery => PostgresQueries.UserDefinedTypeDefinitionQuery;
        public override string StoredProcedureParameterQuery => PostgresQueries.StoredProcedureParameterQuery;
        public override string ViewColumnQuery => PostgresQueries.ViewColumnQuery;
        public override string FunctionParameterQuery => PostgresQueries.FunctionDefinitionQuery;
        public override string UserDefinedTypeColumnQuery => PostgresQueries.UserDefinedTypeColumnQuery;
        public override string CheckConstraintQuery => PostgresQueries.CheckConstraintQuery;
        public override string ExtendedPropertyQuery => PostgresQueries.ExtendedPropertyQuery;
        public override string MissingIndexQuery => PostgresQueries.MissingIndexQuery;
        public override string StatisticQuery => PostgresQueries.StatisticQuery;
        public override string TriggerQuery => PostgresQueries.TriggerQuery;
        public override string SynonymQuery => PostgresQueries.SynonymQuery;
        public override string DatabasePrincipalQuery => PostgresQueries.DatabasePrincipalQuery;
        public override string DatabasePermissionQuery => PostgresQueries.DatabasePermissionQuery;
        public override string DatabaseRoleMembershipQuery => PostgresQueries.DatabaseRoleMembershipQuery;
    }
}
