using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;
public class SaveUserShrinkageStatusException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);

