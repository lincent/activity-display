using System.Net.Http.Headers;
using System.Text.Json;
using StravaApi.Application.Services;
using StravaApi.Infrastructure.Models;

namespace StravaApi.Infrastructure.Services;

public class StravaHttpClient : IStravaHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public StravaHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
    }

    public async Task<StravaTokenResponse> ExchangeCodeAsync(string code, string clientId, string clientSecret)
    {
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code"
        });

        var response = await _httpClient.PostAsync("https://www.strava.com/oauth/token", formData);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<StravaTokenResponse>(json, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize token response");
    }

    public async Task<StravaTokenResponse> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret)
    {
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });

        var response = await _httpClient.PostAsync("https://www.strava.com/oauth/token", formData);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<StravaTokenResponse>(json, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize token response");
    }

    public async Task<List<StravaActivity>> GetActivitiesAsync(string accessToken, int perPage)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var url = $"https://www.strava.com/api/v3/athlete/activities?per_page={perPage}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<StravaActivity>>(json, _jsonOptions) ?? new List<StravaActivity>();
    }
}
