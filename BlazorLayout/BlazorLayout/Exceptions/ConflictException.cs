using BlazorLayout.Extensions;
using System.Net;

namespace BlazorLayout.Exceptions;
public class ConflictException : Exception
{
    public Guid CorrelationId { get; }

    public ConflictException(HttpRequestException ex, Guid correlationId) : base(null, ex)
    {
        CorrelationId = correlationId;
#if DEBUG
        Utils.Assert(ex is { StatusCode: HttpStatusCode.Conflict });
#endif
    }
}

