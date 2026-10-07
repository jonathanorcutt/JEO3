namespace JEO3.Schema
{
    public interface IStatistic : IObject
    {
        long? ParentObjectId { get; }
        int StatsId { get; }
        bool IsAutoCreated { get; }
        bool IsUserCreated { get; }
        bool HasFilter { get; }
        string? FilterDefinition { get; }
        DateTime? CreateDate { get; }
        DateTime? ModifyDate { get; }
        string? CreatedBy { get; }
    }
}
