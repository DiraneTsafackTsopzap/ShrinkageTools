namespace BlazorLayout.Modeles
{
    public class UserDto
    {
        public Guid UserId { get; init; }
        public Guid? TeamId { get; init; }
        public string Email { get; init; } = null!;
        public IReadOnlyList<PaidTime> PaidTimeList { get; init; } = null!;
    }

    public record PaidTime
    {
        public Guid Id { get; init; }
        public TimeSpan PaidTimeMonday { get; init; }
        public TimeSpan PaidTimeTuesday { get; init; }
        public TimeSpan PaidTimeWednesday { get; init; }
        public TimeSpan PaidTimeThursday { get; init; }
        public TimeSpan PaidTimeFriday { get; init; }
        public TimeSpan PaidTimeSaturday { get; init; }
        public DateOnly ValidFrom { get; init; }
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = null!;
    }
}
