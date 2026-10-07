namespace JEO3.Engine
{
    public sealed class OracleQueryCollection
    {
        public string TableQuery => OracleQueries.TableQuery;
        public string ColumnQuery => OracleQueries.ColumnQuery;
        public string RelationQuery => OracleQueries.RelationQuery;
        public string IndexQuery => OracleQueries.IndexQuery;
        public string StoredProcedureDefinitionQuery => OracleQueries.StoredProcedureDefinitionQuery;
        public string ViewDefinitionQuery => OracleQueries.ViewDefinitionQuery;
        public string FunctionDefinitionQuery => OracleQueries.FunctionDefinitionQuery;
        public string UserDefinedTypeDefinitionQuery => OracleQueries.UserDefinedTypeDefinitionQuery;

        public string StoredProcedureParameterQuery => OracleQueries.StoredProcedureParameterQuery;
        public string ViewColumnQuery => OracleQueries.ViewColumnQuery;
        public string FunctionParameterQuery => OracleQueries.FunctionDefinitionQuery;
        public string UserDefinedTypeColumnQuery => OracleQueries.UserDefinedTypeColumnQuery;
        public string CheckConstraintQuery => OracleQueries.CheckConstraintQuery;
        public string ExtendedPropertyQuery => OracleQueries.ExtendedPropertyQuery;
        public string MissingIndexQuery => OracleQueries.MissingIndexQuery;
        public string StatisticQuery => OracleQueries.StatisticQuery;
        public string TriggerQuery => OracleQueries.TriggerQuery;
        public string SynonymQuery => OracleQueries.SynonymQuery;
        public string DatabasePrincipalQuery => OracleQueries.DatabasePrincipalQuery;
        public string DatabasePermissionQuery => OracleQueries.DatabasePermissionQuery;
        public string DatabaseRoleMembershipQuery => OracleQueries.DatabaseRoleMembershipQuery;

        // Diagnostic
        //public string BlockingQuery => MSSQLQueries.BlockingQuery;
        //public string WaitTimeQuery => MSSQLQueries.WaitTimeQuery;
        //public string WaitStatsSnapshotQuery => MSSQLQueries.WaitStatsSnapshotQuery;
        //public string ActiveRequestsQuery => MSSQLQueries.ActiveRequestsQuery;
        //public string GetDeadlockQuery(DateTime sinceUtc) => MSSQLQueries.GetDeadlockQuery(sinceUtc);

    }
}
