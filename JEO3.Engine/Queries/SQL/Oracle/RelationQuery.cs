namespace JEO3.Engine
{
    internal static partial class OracleQueries
    {
        internal const string RelationQuery = @"
SELECT 
    p_cols.OWNER PrimarySchema,
    p_cols.TABLE_NAME PrimaryTableName,
    p_cols.COLUMN_NAME PrimaryColumnName,
    p_cols.OWNER || '.' || p_cols.TABLE_NAME || '.' || p_cols.COLUMN_NAME PrimaryColumnKey,
    f_cols.OWNER || '.' || f_cols.TABLE_NAME || '.' || f_cols.COLUMN_NAME ForeignColumnKey,
    p_tab_cols.DATA_TYPE PrimaryColumnDataType,
    f_cols.OWNER ForeignSchema,
    f_cols.TABLE_NAME ForeignTableName,
    f_cols.COLUMN_NAME ForeignColumnName,
    f_tab_cols.DATA_TYPE ForeignColumnDataType,
    f_cons.CONSTRAINT_NAME ForeignKeyName,
    CASE WHEN f_tab_cols.NULLABLE = 'Y' THEN 1 ELSE 0 END ForeignIsNullable,
    CASE WHEN fk_count.col_count > 1 THEN 1 ELSE 0 END IsCompositeKey,
    -- ^ Main Schema Metadata --
    p_cols.OWNER || '.' || p_cols.TABLE_NAME PrimaryTableFullName,
    p_cols.OWNER || '.' || p_cols.TABLE_NAME || '.' || p_cols.COLUMN_NAME PrimaryColumnFullName,
    f_cols.OWNER || '.' || f_cols.TABLE_NAME ForeignTableFullName,
    f_cols.OWNER || '.' || f_cols.TABLE_NAME || '.' || f_cols.COLUMN_NAME ForeignColumnFullName
FROM all_constraints f_cons
INNER JOIN all_cons_columns f_cols ON f_cons.CONSTRAINT_NAME = f_cols.CONSTRAINT_NAME AND f_cons.OWNER = f_cols.OWNER
INNER JOIN all_constraints p_cons ON f_cons.R_CONSTRAINT_NAME = p_cons.CONSTRAINT_NAME AND f_cons.R_OWNER = p_cons.OWNER
INNER JOIN all_cons_columns p_cols ON p_cons.CONSTRAINT_NAME = p_cols.CONSTRAINT_NAME AND p_cons.OWNER = p_cols.OWNER AND p_cols.POSITION = f_cols.POSITION
INNER JOIN all_tab_cols p_tab_cols ON p_tab_cols.OWNER = p_cols.OWNER AND p_tab_cols.TABLE_NAME = p_cols.TABLE_NAME AND p_tab_cols.COLUMN_NAME = p_cols.COLUMN_NAME
INNER JOIN all_tab_cols f_tab_cols ON f_tab_cols.OWNER = f_cols.OWNER AND f_tab_cols.TABLE_NAME = f_cols.TABLE_NAME AND f_tab_cols.COLUMN_NAME = f_cols.COLUMN_NAME
INNER JOIN (
    SELECT CONSTRAINT_NAME, OWNER, COUNT(*) as col_count
    FROM all_cons_columns
    WHERE OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
    GROUP BY CONSTRAINT_NAME, OWNER
) fk_count ON fk_count.CONSTRAINT_NAME = f_cons.CONSTRAINT_NAME AND fk_count.OWNER = f_cons.OWNER
WHERE 
    f_cons.CONSTRAINT_TYPE = 'R'
    AND f_cons.OWNER = sys_context('USERENV', 'CURRENT_SCHEMA') -- SPEED FIX HERE
    AND f_cons.OWNER = 'ORDDATA' -- HARDCODE FOR LAPTOP
ORDER BY 
    p_cols.OWNER, 
    p_cols.TABLE_NAME, 
    p_cols.COLUMN_NAME, 
    f_cols.OWNER, 
    f_cols.TABLE_NAME, 
    f_cols.COLUMN_NAME";
    }
}
