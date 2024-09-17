namespace AIDogConsole;
using System.Net.Http;
using System.Net.Http.Json;

internal class AIDogClient
{
    private readonly HttpClient _httpClient;
    internal AIDogClient(HttpClient httpClient, Uri endpoint)
    {
        this._httpClient = httpClient;
        this._httpClient.BaseAddress = endpoint;
    }

    internal async Task<ReadOnlyMemory<byte>> GetSightAsync()
    {
        var response = await this._httpClient.GetAsync("/sight");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsByteArrayAsync();
        return content;
    }

    internal async Task MoveForwardAsync(int distanceInCentimeters)
    {
        var response = await this._httpClient.PostAsync("/forward", JsonContent.Create(new { distance = distanceInCentimeters }));
        response.EnsureSuccessStatusCode();
    }

    internal async Task TurnLeftAsync(int degrees)
    {
        var response = await this._httpClient.PostAsync("/turn", JsonContent.Create(new { theta = Math.Abs(degrees) }));
        response.EnsureSuccessStatusCode();
    }
}