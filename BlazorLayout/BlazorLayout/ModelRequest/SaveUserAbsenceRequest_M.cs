using BlazorLayout.Modeles;

namespace BlazorLayout.ModelRequest;

public class SaveUserAbsenceRequest_M
{
    public Guid CorrelationId { get; init; }
    public AbsenceDto Absence { get; init; } = null!;
}
