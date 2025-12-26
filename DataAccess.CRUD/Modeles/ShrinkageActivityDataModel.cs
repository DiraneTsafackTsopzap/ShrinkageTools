namespace DataAccess.CRUD.Modeles;
public class ShrinkageActivityDataModel
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTime DeletedAt { get; init; }
    public Guid DeletedBy { get; init; }
    public Guid UserId { get; init; }
    public Guid TeamId { get; init; }
    //public DateTimeOffset StartedAt { get; init; }
    //public DateTimeOffset? StoppedAt { get; init; }



    // ✅ UTC ONLY
    public DateTime StartedAt { get; init; }
    public DateTime? StoppedAt { get; init; }
    public string ActivityType { get; init; } = null!;
    public string ActivityTrackType { get; init; } = null!;
}

