using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;
public class SaveAbsenceException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);


