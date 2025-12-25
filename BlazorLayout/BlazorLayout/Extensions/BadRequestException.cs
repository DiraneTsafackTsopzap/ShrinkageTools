using System.Net;

namespace BlazorLayout.Extensions
{
    public class BadRequestException : Exception
    {
        public Guid CorrelationId { get; }

        public BadRequestException(HttpRequestException ex, Guid correlationId) : base(null, ex)
        {
            CorrelationId = correlationId;
#if DEBUG
            Utils.Assert(ex is { StatusCode: HttpStatusCode.BadRequest });
#endif
        }
    }


    public class NotFoundException : Exception
    {
        public Guid CorrelationId { get; }

        public NotFoundException(HttpRequestException ex, Guid correlationId) : base(null, ex)
        {
            CorrelationId = correlationId;
#if DEBUG
            Utils.Assert(ex is { StatusCode: HttpStatusCode.NotFound });
#endif
        }
    }


}
