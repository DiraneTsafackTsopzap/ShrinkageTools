namespace BlazorLayout.Extensions;

public class SaveActivityException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);

