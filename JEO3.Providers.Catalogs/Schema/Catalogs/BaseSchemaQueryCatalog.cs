namespace JEO3.Providers.Catalogs.Catalogs
{
    public abstract class BaseSchemaQueryCatalog : ISchemaQueryCatalog
    {
        public abstract string DatabaseQuery { get; }
        public abstract string SchemaQuery { get; }
        public abstract string TableQuery { get; }
        public abstract string ColumnQuery { get; }
        public abstract string RelationQuery { get; }
        public abstract string IndexQuery { get; }
        public abstract string StoredProcedureDefinitionQuery { get; }
        public abstract string ViewDefinitionQuery { get; }
        public abstract string FunctionDefinitionQuery { get; }
        public abstract string UserDefinedTypeDefinitionQuery { get; }
        public abstract string StoredProcedureParameterQuery { get; }
        public abstract string FunctionParameterQuery { get; }
        public abstract string ViewColumnQuery { get; }
        public abstract string UserDefinedTypeColumnQuery { get; }
        public abstract string CheckConstraintQuery { get; }
        public abstract string ExtendedPropertyQuery { get; }
        public abstract string MissingIndexQuery { get; }
        public abstract string StatisticQuery { get; }
        public abstract string TriggerQuery { get; }
        public abstract string SynonymQuery { get; }
        public abstract string DatabasePrincipalQuery { get; }
        public abstract string DatabasePermissionQuery { get; }
        public abstract string DatabaseRoleMembershipQuery { get; }


        public string GetQuery(SchemaQueryType queryType)
        {
            switch (queryType)
            {
                case SchemaQueryType.DatabaseQuery:
                    return DatabaseQuery;
                case SchemaQueryType.SchemaQuery:
                    return SchemaQuery;
                case SchemaQueryType.TableQuery:
                    return TableQuery;
                case SchemaQueryType.ColumnQuery:
                    return ColumnQuery;
                case SchemaQueryType.DatabasePermissionQuery:
                    return DatabasePermissionQuery;
                case SchemaQueryType.DatabasePrincipalQuery:
                    return DatabasePrincipalQuery;
                case SchemaQueryType.DatabaseRoleMembershipQuery:
                    return DatabaseRoleMembershipQuery;
                case SchemaQueryType.ExtendedPropertyQuery:
                    return ExtendedPropertyQuery;
                case SchemaQueryType.FunctionDefinitionQuery:
                    return FunctionDefinitionQuery;
                case SchemaQueryType.FunctionParameterQuery:
                    return FunctionParameterQuery;
                case SchemaQueryType.IndexQuery:
                    return IndexQuery;
                case SchemaQueryType.MissingIndexQuery:
                    return MissingIndexQuery;
                case SchemaQueryType.RelationQuery:
                    return RelationQuery;
                case SchemaQueryType.StatisticQuery:
                    return StatisticQuery;
                case SchemaQueryType.StoredProcedureDefinitionQuery:
                    return StoredProcedureDefinitionQuery;
                case SchemaQueryType.StoredProcedureParameterQuery:
                    return StoredProcedureParameterQuery;
                case SchemaQueryType.SynonymQuery:
                    return SynonymQuery;
                case SchemaQueryType.TriggerQuery:
                    return TriggerQuery;
                case SchemaQueryType.UserDefinedTypeColumnQuery:
                    return UserDefinedTypeColumnQuery;
                case SchemaQueryType.UserDefinedTypeDefinitionQuery:
                    return UserDefinedTypeDefinitionQuery;
                case SchemaQueryType.ViewColumnQuery:
                    return ViewColumnQuery;
                case SchemaQueryType.ViewDefinitionQuery:
                    return ViewDefinitionQuery;
                case SchemaQueryType.CheckConstraintQuery:
                    return CheckConstraintQuery;
            }
            return string.Empty;
        }
    }
}
