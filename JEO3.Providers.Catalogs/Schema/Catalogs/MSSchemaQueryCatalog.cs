using JEO3.Providers.Catalogs.Catalogs;

namespace JEO3.Providers.Catalogs
{
    public sealed class MSSchemaQueryCatalog : BaseSchemaQueryCatalog
    {
        public override string DatabaseQuery => MSSQLQueries.DatabaseQuery;
        public override string SchemaQuery => MSSQLQueries.SchemaQuery;
        public override string TableQuery => MSSQLQueries.TableQuery;
        public override string ColumnQuery => MSSQLQueries.ColumnQuery;
        public override string RelationQuery => MSSQLQueries.RelationQuery;
        public override string IndexQuery => MSSQLQueries.IndexQuery;
        public override string StoredProcedureDefinitionQuery => MSSQLQueries.StoredProcedureDefinitionQuery;
        public override string ViewDefinitionQuery => MSSQLQueries.ViewDefinitionQuery;
        public override string FunctionDefinitionQuery => MSSQLQueries.FunctionDefinitionQuery;
        public override string UserDefinedTypeDefinitionQuery => MSSQLQueries.UserDefinedTypeDefinitionQuery;
        public override string StoredProcedureParameterQuery => MSSQLQueries.StoredProcedureParameterQuery;
        public override string ViewColumnQuery => MSSQLQueries.ViewColumnQuery;
        public override string FunctionParameterQuery => MSSQLQueries.FunctionParameterQuery;
        public override string UserDefinedTypeColumnQuery => MSSQLQueries.UserDefinedTypeColumnQuery;
        public override string CheckConstraintQuery => MSSQLQueries.CheckConstraintQuery;
        public override string ExtendedPropertyQuery => MSSQLQueries.ExtendedPropertyQuery;
        public override string MissingIndexQuery => MSSQLQueries.MissingIndexQuery;
        public override string StatisticQuery => MSSQLQueries.StatisticQuery;
        public override string TriggerQuery => MSSQLQueries.TriggerQuery;
        public override string SynonymQuery => MSSQLQueries.SynonymQuery;
        public override string DatabasePrincipalQuery => MSSQLQueries.DatabasePrincipalQuery;
        public override string DatabasePermissionQuery => MSSQLQueries.DatabasePermissionQuery;
        public override string DatabaseRoleMembershipQuery => MSSQLQueries.DatabaseRoleMembershipQuery;
    }
}
