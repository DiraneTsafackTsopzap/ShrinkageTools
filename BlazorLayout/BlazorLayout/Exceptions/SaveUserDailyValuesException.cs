using BlazorLayout.Extensions;

namespace BlazorLayout.Exceptions;
  
public class SaveUserDailyValuesException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);

