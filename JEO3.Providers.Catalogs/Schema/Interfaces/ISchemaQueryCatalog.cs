namespace JEO3.Providers.Catalogs
{
    public interface ISchemaQueryCatalog
    {
        string DatabaseQuery { get; }
        string SchemaQuery { get; }
        string TableQuery { get; }
        string ColumnQuery { get; }
        string RelationQuery { get; }
        string IndexQuery { get; }
        string StoredProcedureDefinitionQuery { get; }
        string ViewDefinitionQuery { get; }
        string FunctionDefinitionQuery { get; }
        string UserDefinedTypeDefinitionQuery { get; }

        string StoredProcedureParameterQuery { get; }
        string FunctionParameterQuery { get; }
        string ViewColumnQuery { get; }
        string UserDefinedTypeColumnQuery { get; }

        string CheckConstraintQuery { get; }
        string ExtendedPropertyQuery { get; }
        string MissingIndexQuery { get; }
        string StatisticQuery { get; }
        string TriggerQuery { get; }
        string SynonymQuery { get; }
        string DatabasePrincipalQuery { get; }
        string DatabasePermissionQuery { get; }
        string DatabaseRoleMembershipQuery { get; }

        string GetQuery(SchemaQueryType queryType);
    }
}
