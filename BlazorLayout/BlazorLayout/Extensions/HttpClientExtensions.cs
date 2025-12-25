using System.Net.Http.Json;

namespace BlazorLayout.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<TValue> GetFromJsonAsyncNotNull<TValue>(this HttpClient client, string requestUri, CancellationToken cancellationToken) =>
            await client.GetFromJsonAsync<TValue>(requestUri, cancellationToken) ?? throw new InvalidOperationException("Bad API response.");

        public static async Task<T> ReadFromJsonAsyncNotNull<T>(this HttpContent content, CancellationToken cancellationToken)
        {
            var response = await content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return response ?? throw new InvalidOperationException("Bad API response");
        }

        public static async Task<HttpResponseMessage> DeleteJsonAsync<TValue>(this HttpClient client, string requestUri, TValue value, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
            {
                Content = JsonContent.Create(value),
            };
            return await client.SendAsync(request, cancellationToken);
        }
    }

}
