namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string TriggerQuery = @"
SELECT
    o.OBJECT_ID AS ObjectId,
    po.OBJECT_ID AS ParentObjectId,
    tr.TRIGGER_NAME AS Name,
    CASE WHEN tr.STATUS = 'DISABLED' THEN 1 ELSE 0 END AS IsDisabled,
    CASE WHEN tr.TRIGGER_TYPE = 'INSTEAD OF' THEN 1 ELSE 0 END AS IsInsteadOfTrigger,
    'CREATE OR REPLACE ' || src.Definition AS Definition,
    STANDARD_HASH('CREATE OR REPLACE ' || src.Definition, 'SHA256') AS DefinitionHash
FROM all_triggers tr
INNER JOIN all_users u ON u.USERNAME = tr.OWNER AND u.ORACLE_MAINTAINED = 'N'
INNER JOIN all_objects o ON o.OWNER = tr.OWNER AND o.OBJECT_NAME = tr.TRIGGER_NAME AND o.OBJECT_TYPE = 'TRIGGER'
INNER JOIN all_objects po ON po.OWNER = tr.TABLE_OWNER AND po.OBJECT_NAME = tr.TABLE_NAME AND po.OBJECT_TYPE IN ('TABLE', 'VIEW')
LEFT JOIN (
    SELECT OWNER, NAME, DBMS_XMLGEN.CONVERT(XMLAGG(XMLELEMENT(e, TEXT) ORDER BY LINE).EXTRACT('//text()').GETCLOBVAL(), 1) AS Definition
    FROM all_source
    WHERE TYPE = 'TRIGGER'
    GROUP BY OWNER, NAME
) src ON src.OWNER = tr.OWNER AND src.NAME = tr.TRIGGER_NAME
WHERE tr.BASE_OBJECT_TYPE IN ('TABLE', 'VIEW')
    AND tr.TRIGGER_NAME NOT LIKE 'BIN$%'
    --AND tr.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
ORDER BY tr.OWNER, tr.TRIGGER_NAME";
    }
}