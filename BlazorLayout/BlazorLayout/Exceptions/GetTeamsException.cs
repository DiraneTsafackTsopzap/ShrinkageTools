using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;
public class GetTeamsException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);
