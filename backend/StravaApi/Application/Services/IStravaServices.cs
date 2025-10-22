using StravaApi.Application.DTOs;
using StravaApi.Infrastructure.Models;

namespace StravaApi.Application.Services;

public interface IStravaAuthService
{
    string BuildAuthorizeUrl();
    Task ExchangeCodeAsync(string code);
    Task<string> GetValidAccessTokenAsync();
}

public interface IStravaApiService
{
    Task<List<RunDto>> GetRunningActivitiesAsync(string accessToken, int perPage);
}

public interface ITokenStore
{
    Task<TokenState?> ReadAsync();
    Task WriteAsync(TokenState state);
}

public interface IStravaHttpClient
{
    Task<StravaTokenResponse> ExchangeCodeAsync(string code, string clientId, string clientSecret);
    Task<StravaTokenResponse> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret);
    Task<List<StravaActivity>> GetActivitiesAsync(string accessToken, int perPage);
}
