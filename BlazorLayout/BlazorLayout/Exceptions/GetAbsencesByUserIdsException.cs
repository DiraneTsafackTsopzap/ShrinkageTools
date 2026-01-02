using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;
public class GetAbsencesByUserIdsException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);


