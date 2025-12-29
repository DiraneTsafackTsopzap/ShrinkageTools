using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;

public class GetUserDailySummaryException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);

