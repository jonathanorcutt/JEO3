namespace JEO3.Providers.Catalogs
{
    public enum SchemaQueryType
    {
        DatabaseQuery,
        SchemaQuery,
        TableQuery,
        ColumnQuery,
        RelationQuery,
        IndexQuery,
        StoredProcedureDefinitionQuery,
        ViewDefinitionQuery,
        FunctionDefinitionQuery,
        UserDefinedTypeDefinitionQuery,
        StoredProcedureParameterQuery,
        FunctionParameterQuery,
        ViewColumnQuery,
        UserDefinedTypeColumnQuery,
        CheckConstraintQuery,
        ExtendedPropertyQuery,
        MissingIndexQuery,
        StatisticQuery,
        TriggerQuery,
        SynonymQuery,
        DatabasePrincipalQuery,
        DatabasePermissionQuery,
        DatabaseRoleMembershipQuery
    }

    public enum MonitorQueryType
    {
        ActiveRequestsQuery,
        //DeadlockQuery, // Parameter Value Required sinceUtc
        WaitStatsSnapshotQuery,
        BlockingQuery,
        TopExpensiveQueryQuery,
        PerformanceMetricQuery
    }

    public enum MonitorStoreQueryType
    {
        ActiveRequestsQuery,
        WaitSamplesQuery,
        WaitTypesQuery,
        DeadlockEventQuery,
        DeadlockParticipantQuery,
        BlockingQuery,
        TopExpensiveQueryQuery,
        PerformanceMetricQuery
    }

    public enum SQliteQueryType
    {
        InitSqliteSchemaQuery,
        PurgeHistoryAfter7Days
    }
}
