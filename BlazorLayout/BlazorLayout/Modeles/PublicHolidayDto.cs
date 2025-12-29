namespace BlazorLayout.Modeles
{
    public record PublicHolidayDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public DateOnly AffectedDate { get; init; }
    }
}
