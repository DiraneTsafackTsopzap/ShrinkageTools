namespace BlazorLayout.Extensions
{
    public class CorrelatedException : Exception
    {
        public Guid? CorrelationId { get; }

        protected CorrelatedException(string? message, Exception? innerException, Guid? correlationId) : base(message, innerException)
        {
            CorrelationId = correlationId;
        }
    }
}
