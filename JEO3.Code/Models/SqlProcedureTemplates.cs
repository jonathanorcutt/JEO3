namespace JEO3.Code
{
    public static class SqlProcedureTemplates
    {
        /// <summary>
        /// {0} = Entity Name (e.g., "Claim")
        /// {1} = SP Parameters (e.g., "@Name VARCHAR(50), @Amount DECIMAL(18,2)")
        /// {2} = Insert Columns (e.g., "[Name], [Amount]")
        /// {3} = Insert Values (e.g., "@Name, @Amount")
        /// </summary>
        public const string SpInsert = @"
CREATE PROCEDURE [{0}].[spInsert{1}]
(
    {2}
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [{0}].[{1}] ({3})
    VALUES ({4});

    SELECT CAST(SCOPE_IDENTITY() AS INT);
END
GO";
        /// <summary>
        /// {0} = Entity Name
        /// {1} = SP Parameters (e.g., "@Id INT, @Name VARCHAR(50)")
        /// {2} = Update Set Expressions (e.g., "[Name] = @Name, [Amount] = @Amount")
        /// {3} = Where Clause (e.g., "[Id] = @Id")
        /// </summary>
        public const string SpUpdate = @"
CREATE PROCEDURE [{0}].[spUpdate{1}]
(
    {2}
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [{0}].[{1}]
    SET {3}
    WHERE {4};
END
GO";
        /// <summary>
        /// {0} = Entity Name
        /// {1} = SP Parameters (e.g., "@Id INT")
        /// {2} = Where Clause (e.g., "[Id] = @Id")
        /// </summary>
        public const string SpDelete = @"
CREATE PROCEDURE [{0}].[spDelete{1}]
(
    {2}
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{0}].[{1}]
    WHERE {3};
END
GO";
        /// <summary>
        /// {0} = Entity Name
        /// {1} = SP Parameters (e.g., "@Id INT")
        /// {2} = Select Columns (e.g., "*" or "[Id], [Name]")
        /// {3} = Where Clause (e.g., "[Id] = @Id")
        /// </summary>
        public const string SpGetById = @"
CREATE PROCEDURE [{0}].[spGet{1}ById]
(
    {2}
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT {3}
    FROM [{0}].[{1}]
    WHERE {4};
END
GO";
        /// <summary>
        /// {0} = Entity Name
        /// {1} = SP Parameters (e.g., "@Ids VARCHAR(MAX)" for CSV, or "@Ids [dbo].[IntListType] READONLY" for TVP)
        /// {2} = Select Columns (e.g., "*")
        /// {3} = Where Clause (e.g., "[Id] IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@Ids, ','))")
        /// </summary>
        public const string SpGetByIds = @"
CREATE PROCEDURE [{0}].[spGet{1}ByIds]
(
    @IdsJson NVARCHAR(MAX)
)
AS
BEGIN
    SET NOCOUNT ON;

    WITH Ids AS 
    (
        SELECT {2} FROM OPENJSON(@IdsJson) WITH ( {2} {3} '$' )
)   

    SELECT {4}
    FROM [{0}].[{1}] X
    INNER JOIN Ids Ids ON Ids.{2} = X.[{2}];
END
GO";

    }
}