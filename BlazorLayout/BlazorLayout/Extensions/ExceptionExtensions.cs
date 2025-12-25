using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Net;

namespace BlazorLayout.Extensions;

    public static class ExceptionExtensions
    {
    [Inject]
    public static IStringLocalizer Localizer { private get; set; } = null!;
    public static string? GetReasonMessage(this HttpRequestException ex, Exception? parent)
    {
        var baseMessage = ex switch
        {
            { StatusCode: HttpStatusCode.BadRequest } => Localizer["http_400"],
            { StatusCode: HttpStatusCode.Forbidden } => Localizer["http_403"],
            { StatusCode: HttpStatusCode.NotFound } => Localizer["http_404"],
            { StatusCode: >= (HttpStatusCode)400 and <= (HttpStatusCode)499 and var statusCode } => Localizer["http_4XX", (int)statusCode],
            { StatusCode: HttpStatusCode.InternalServerError } => Localizer["http_500"],
            { StatusCode: HttpStatusCode.BadGateway } => Localizer["http_502"],
            { StatusCode: HttpStatusCode.ServiceUnavailable } => Localizer["http_503"],
            { StatusCode: HttpStatusCode.GatewayTimeout } => Localizer["http_504"],
            { StatusCode: >= (HttpStatusCode)500 and <= (HttpStatusCode)599 and var statusCode } => Localizer["http_5XX", (int)statusCode],
            _ => (string?)null,
        };
        return (parent ?? ex).AppendCorrelationId(baseMessage);
    }

    private static string? AppendCorrelationId(this Exception ex, string? message)
    {
        if (ex is CorrelatedException correlatedException && correlatedException.CorrelationId != null)
        {
            if (message != null)
                return message + Localizer["correlation_id_G", correlatedException.CorrelationId];

            return Localizer["correlation_id_G", correlatedException.CorrelationId];
        }

        return message;
    }
}

