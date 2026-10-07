namespace JEO3.Schema
{
    public enum DatabaseObjectType
    {
        Unknown = 0,
        Column,
        Constraint,
        Database,
        ExtentedProperty,
        Index,
        ForeignKey,
        Function,
        MissingIndex,
        Procedure,
        Table,
        Trigger,
        Schema,
        Statistic,
        Synonym,
        View,
        UserDefinedType
    }

    public enum RelationType
    {
        Unknown = 0,
        ForeignKey,
        Reference,
        Dependency
    }
    public enum IndexType
    {
        Unknown = 0,
        Heap,
        Clustered,
        NonClustered,
        Hash,
        ColumnStore,
        Other
    }
    public enum IndexColumnSortDirection
    {
        Unknown = 0,
        Ascending,
        Descending
    }
    public enum FunctionType
    {
        Unknown = 0,
        Scalar,
        TableValued,
        Aggregate,
        InlineTableValued
    }
    public enum UserDefinedTypeKind
    {
        Unknown = 0,
        Alias,       // ALIAS_UDT — scalar alias over a base type, e.g. AW's Name = nvarchar(50)
        Structured,  // TABLE_TYPE — TVP-style table type with real column schema
        ClrType,     // CLR_UDT — assembly-backed or system CLR (hierarchyid, geography, geometry)
        Enum         // reserved for Postgres
    }
}