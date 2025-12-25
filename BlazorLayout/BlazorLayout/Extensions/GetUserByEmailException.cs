namespace BlazorLayout.Extensions
{
    public class GetUserByEmailException(Exception ex, Guid correlationId) : CorrelatedException(null, ex, correlationId);



}
