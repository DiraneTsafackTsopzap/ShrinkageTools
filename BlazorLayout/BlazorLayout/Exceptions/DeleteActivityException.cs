using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;

public class DeleteActivityException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);