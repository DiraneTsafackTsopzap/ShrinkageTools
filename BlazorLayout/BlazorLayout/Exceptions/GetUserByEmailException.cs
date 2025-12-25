using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;

public class GetUserByEmailException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);




