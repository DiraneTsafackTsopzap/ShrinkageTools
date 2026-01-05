using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;
public class DeleteAbsenceException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);
