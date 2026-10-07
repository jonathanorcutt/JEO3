namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string RelationQuery = @"
SELECT DISTINCT
    FK.object_id AS ObjectId,
    FK.create_date AS KeyCreatedDate,
    FK.modify_date AS KeyModifiedDate,
    FK.key_index_id AS KeyIndexId,
    FK.type_desc AS KeyType,
    FK.name AS KeyName,
    (CASE WHEN FKC_COUNT.ColumnCount > 1 THEN 1 ELSE 0 END) AS IsComposite,
    CF.is_nullable AS IsNullable,
    FK.is_disabled AS IsDisabled,
    FK.is_not_trusted AS IsNotTrusted,
    CF.is_computed AS IsComputed,
    FK.delete_referential_action_desc AS DeleteAction,
    FK.update_referential_action_desc AS UpdateAction,
    SP.name AS PrimarySchema,
    TP.name AS PrimaryTableName,
    CP.name AS PrimaryColumnName,
    TYP.name AS PrimaryDataType,
    CONCAT(SP.name, '.', TP.name, '.', CP.name) AS PrimaryColumnPath,
    CONCAT(SF.name, '.', TF.name, '.', CF.name) AS ForeignColumnPath,
    SF.name AS ForeignSchema,
    TF.name AS ForeignTableName,
    CF.name AS ForeignColumnName,
    TYF.name AS ForeignDataType,
    CONCAT(SP.name, '.', TP.name) AS PrimaryTablePath,
    CONCAT(SF.name, '.', TF.name) AS ForeignTablePath
FROM sys.foreign_key_columns FKC WITH (NOLOCK)
    INNER JOIN sys.foreign_keys FK WITH (NOLOCK) ON FK.object_id = FKC.constraint_object_id
    INNER JOIN sys.tables TP WITH (NOLOCK) ON TP.object_id = FKC.referenced_object_id
    INNER JOIN sys.tables TF WITH (NOLOCK) ON TF.object_id = FKC.parent_object_id
    INNER JOIN sys.schemas SP WITH (NOLOCK) ON SP.schema_id = TP.schema_id
    INNER JOIN sys.schemas SF WITH (NOLOCK) ON SF.schema_id = TF.schema_id
    INNER JOIN sys.columns CP WITH (NOLOCK) ON CP.object_id = TP.object_id AND CP.column_id = FKC.referenced_column_id
    INNER JOIN sys.columns CF WITH (NOLOCK) ON CF.object_id = TF.object_id AND CF.column_id = FKC.parent_column_id
    INNER JOIN sys.types TYP WITH (NOLOCK) ON TYP.user_type_id = CP.user_type_id
    INNER JOIN sys.types TYF WITH (NOLOCK) ON TYF.user_type_id = CF.user_type_id
    INNER JOIN
    (
        SELECT
            constraint_object_id,
            COUNT(*) AS ColumnCount
        FROM sys.foreign_key_columns WITH (NOLOCK)
        GROUP BY constraint_object_id
    ) FKC_COUNT ON FKC_COUNT.constraint_object_id = FK.object_id
--ORDER BY
--    SP.name,
--    TP.name,
--    CP.name,
--    SF.name,
--    TF.name,
--    CF.name;";
    }
}
