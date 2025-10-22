using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace StravaApi;

public interface IStravaAuthService
{
  string BuildAuthorizeUrl();
  Task ExchangeCodeAsync(string code);
  Task<string> GetValidAccessTokenAsync();
}

public class StravaAuthService : IStravaAuthService
{
  private readonly IHttpClientFactory _clientFactory;
  private readonly IOptions<StravaOptions> _options;
  private readonly ITokenStore _tokenStore;

  public StravaAuthService(IHttpClientFactory clientFactory, IOptions<StravaOptions> options, ITokenStore tokenStore)
  {
    _clientFactory = clientFactory;
    _options = options;
    _tokenStore = tokenStore;
  }

  public string BuildAuthorizeUrl()
  {
    var o = _options.Value;
    var url = $"https://www.strava.com/oauth/authorize?client_id={o.ClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(o.RedirectUri)}&approval_prompt=auto&scope=read,activity:read_all";
    return url;
  }

  public async Task ExchangeCodeAsync(string code)
  {
    var o = _options.Value;
    using var client = _clientFactory.CreateClient();
    var form = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["client_id"] = o.ClientId,
      ["client_secret"] = o.ClientSecret,
      ["code"] = code,
      ["grant_type"] = "authorization_code"
    });
    var res = await client.PostAsync("https://www.strava.com/oauth/token", form);
    res.EnsureSuccessStatusCode();
    var json = await res.Content.ReadAsStringAsync();
    var envelope = JsonSerializer.Deserialize<TokenEnvelope>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    var state = new AccessTokenState
    {
      AccessToken = envelope.AccessToken,
      ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(envelope.ExpiresAt),
      RefreshToken = envelope.RefreshToken
    };
    await _tokenStore.WriteAsync(state);
  }

  public async Task<string> GetValidAccessTokenAsync()
  {
    var state = await _tokenStore.ReadAsync();
    if (state == null || state.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(1))
    {
      if (state == null || string.IsNullOrEmpty(state.RefreshToken))
      {
        throw new InvalidOperationException("Not authorized: refresh token missing. Visit /api/strava/auth-url");
      }
      state = await RefreshAsync(state.RefreshToken);
      await _tokenStore.WriteAsync(state);
    }
    return state.AccessToken;
  }

  private async Task<AccessTokenState> RefreshAsync(string refreshToken)
  {
    var o = _options.Value;
    using var client = _clientFactory.CreateClient();
    var form = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["client_id"] = o.ClientId,
      ["client_secret"] = o.ClientSecret,
      ["refresh_token"] = refreshToken,
      ["grant_type"] = "refresh_token"
    });
    var res = await client.PostAsync("https://www.strava.com/oauth/token", form);
    res.EnsureSuccessStatusCode();
    var json = await res.Content.ReadAsStringAsync();
    var envelope = JsonSerializer.Deserialize<TokenEnvelope>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    return new AccessTokenState
    {
      AccessToken = envelope.AccessToken,
      ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(envelope.ExpiresAt),
      RefreshToken = envelope.RefreshToken
    };
  }
}

public interface IStravaApiClient
{
  Task<List<StravaActivity>> GetAthleteActivitiesAsync(string accessToken, int perPage);
}

public class StravaApiClient : IStravaApiClient
{
  private readonly IHttpClientFactory _clientFactory;
  public StravaApiClient(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

  public async Task<List<StravaActivity>> GetAthleteActivitiesAsync(string accessToken, int perPage)
  {
    using var client = _clientFactory.CreateClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    var url = $"https://www.strava.com/api/v3/athlete/activities?per_page={perPage}";
    var res = await client.GetAsync(url);
    res.EnsureSuccessStatusCode();
    var json = await res.Content.ReadAsStringAsync();
    var list = JsonSerializer.Deserialize<List<StravaActivity>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
    return list;
  }
}

public record StravaActivity
(
  long Id,
  string Name,
  string Type,
  string Start_Date_Local,
  double Distance,
  int Moving_Time,
  double Average_Speed
);

public static class ActivityMapper
{
  public static RunDto ToRunDto(StravaActivity a)
  {
    var distanceKm = a.Distance / 1000d;
    var paceMinPerKm = a.Average_Speed > 0 ? (1000d / 60d) / a.Average_Speed : 0;
    return new RunDto
    {
      Id = a.Id,
      Name = a.Name,
      StartDateLocal = a.Start_Date_Local,
      DistanceKm = Math.Round(distanceKm, 2),
      MovingTimeSec = a.Moving_Time,
      AveragePaceMinPerKm = Math.Round(paceMinPerKm, 2)
    };
  }
}

public class RunDto
{
  public long Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string StartDateLocal { get; set; } = string.Empty;
  public double DistanceKm { get; set; }
  public int MovingTimeSec { get; set; }
  public double AveragePaceMinPerKm { get; set; }
}



