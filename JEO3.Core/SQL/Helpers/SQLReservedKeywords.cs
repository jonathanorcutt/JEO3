namespace JEO3.Core
{
    public static class SQLReservedKeywords
    {
        public static readonly List<string> ReservedKeywordsAction =
            ["ADD", "ALTER", "CREATE", "DELETE", "DROP", "INSERT", "SELECT", "UPDATE", "TRUNCATE"];
        public static readonly List<string> ReservedKeywordsStructural =
            ["COLUMN", "DATABASE", "INDEX", "SCHEMA", "TABLE", "VIEW"];
        public static readonly List<string> ReservedKeywordsLogical =
            ["ALL", "AND", "AS", "BY", "CASE", "FROM", "GROUP", "HAVING", "IN", "JOIN", "OR", "ORDER", "WHERE"];
        public static readonly List<string> ReservedKeywordsConstraints =
            ["CHECK", "CONSTRAINT", "DEFAULT", "FOREIGN", "PRIMARY", "REFERENCES", "UNIQUE", "IDENTITY"];
        public static readonly List<string> ReservedKeywordsMetadata =
            ["CURRENT_USER", "IDENTITY", "USER", "SESSION_USER"];
        public static readonly List<string> ReservedKeywordsOther =
            [ "IF", "ON", "WHEN", "END", "GO", "IN", "BEGIN", "FOR", "XML", "CAST", "DESC","ASC","NULL","NOT","EXEC","UNION",
            "FETCH","PIVOT","WITH","CROSS", "LEFT", "RIGHT", "INNER", "OUTER", "FULL", "EXISTS", "BETWEEN", "DISTINCT",
            "LIKE", "IS", "OPENQUERY", "OPENROWSET", "AUDIT", "PROCEDURE" ];
        public static readonly HashSet<string> ReservedKeywordsAll = ReservedKeywordsAction
            .Concat(ReservedKeywordsStructural)
            .Concat(ReservedKeywordsLogical)
            .Concat(ReservedKeywordsConstraints)
            .Concat(ReservedKeywordsMetadata)
            .Concat(ReservedKeywordsOther).ToHashSet<string>();
    }
}
