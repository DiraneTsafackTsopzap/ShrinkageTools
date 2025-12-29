using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;

public class GetUsersShrinkageException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);
