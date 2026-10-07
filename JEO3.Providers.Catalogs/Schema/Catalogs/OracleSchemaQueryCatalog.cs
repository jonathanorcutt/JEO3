using JEO3.Providers.Catalogs.Catalogs;

namespace JEO3.Providers.Catalogs
{
    public sealed class OracleSchemaQueryCatalog : BaseSchemaQueryCatalog
    {
        public override string DatabaseQuery => MSSQLQueries.DatabaseQuery;
        public override string SchemaQuery => MSSQLQueries.SchemaQuery;
        public override string TableQuery => OracleQueries.TableQuery;
        public override string ColumnQuery => OracleQueries.ColumnQuery;
        public override string RelationQuery => OracleQueries.RelationQuery;
        public override string IndexQuery => OracleQueries.IndexQuery;
        public override string StoredProcedureDefinitionQuery => OracleQueries.StoredProcedureDefinitionQuery;
        public override string ViewDefinitionQuery => OracleQueries.ViewDefinitionQuery;
        public override string FunctionDefinitionQuery => OracleQueries.FunctionDefinitionQuery;
        public override string UserDefinedTypeDefinitionQuery => OracleQueries.UserDefinedTypeDefinitionQuery;
        public override string StoredProcedureParameterQuery => OracleQueries.StoredProcedureParameterQuery;
        public override string ViewColumnQuery => OracleQueries.ViewColumnQuery;
        public override string FunctionParameterQuery => OracleQueries.FunctionDefinitionQuery;
        public override string UserDefinedTypeColumnQuery => OracleQueries.UserDefinedTypeColumnQuery;
        public override string CheckConstraintQuery => OracleQueries.CheckConstraintQuery;
        public override string ExtendedPropertyQuery => OracleQueries.ExtendedPropertyQuery;
        public override string MissingIndexQuery => OracleQueries.MissingIndexQuery;
        public override string StatisticQuery => OracleQueries.StatisticQuery;
        public override string TriggerQuery => OracleQueries.TriggerQuery;
        public override string SynonymQuery => OracleQueries.SynonymQuery;
        public override string DatabasePrincipalQuery => OracleQueries.DatabasePrincipalQuery;
        public override string DatabasePermissionQuery => OracleQueries.DatabasePermissionQuery;
        public override string DatabaseRoleMembershipQuery => OracleQueries.DatabaseRoleMembershipQuery;
    }
}
