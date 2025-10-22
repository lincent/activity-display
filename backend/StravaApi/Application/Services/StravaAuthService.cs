using Microsoft.Extensions.Options;
using StravaApi.Application.Services;
using StravaApi.Application.DTOs;
using StravaApi.Application.Exceptions;
using StravaApi.Infrastructure.Configuration;
using StravaApi.Infrastructure.Models;

namespace StravaApi.Application.Services;

public class StravaAuthService : IStravaAuthService
{
    private readonly IOptions<StravaOptions> _options;
    private readonly ITokenStore _tokenStore;
    private readonly IStravaHttpClient _httpClient;

    public StravaAuthService(
        IOptions<StravaOptions> options,
        ITokenStore tokenStore,
        IStravaHttpClient httpClient)
    {
        _options = options;
        _tokenStore = tokenStore;
        _httpClient = httpClient;
    }

    public string BuildAuthorizeUrl()
    {
        var options = _options.Value;
        
        if (string.IsNullOrEmpty(options.ClientId))
        {
            throw new ConfigurationException("Strava ClientId is not configured");
        }

        if (string.IsNullOrEmpty(options.RedirectUri))
        {
            throw new ConfigurationException("Strava RedirectUri is not configured");
        }

        var url = $"https://www.strava.com/oauth/authorize?" +
                  $"client_id={options.ClientId}&" +
                  $"response_type=code&" +
                  $"redirect_uri={Uri.EscapeDataString(options.RedirectUri)}&" +
                  $"approval_prompt=auto&" +
                  $"scope=read,activity:read_all";
        
        return url;
    }

    public async Task ExchangeCodeAsync(string code)
    {
        var options = _options.Value;
        
        if (string.IsNullOrEmpty(options.ClientSecret))
        {
            throw new ConfigurationException("Strava ClientSecret is not configured");
        }

        try
        {
            var response = await _httpClient.ExchangeCodeAsync(code, options.ClientId, options.ClientSecret);
            var state = MapToTokenState(response);
            await _tokenStore.WriteAsync(state);
        }
        catch (HttpRequestException ex)
        {
            throw new StravaApiException("Failed to exchange authorization code", ex);
        }
    }

    public async Task<string> GetValidAccessTokenAsync()
    {
        var state = await _tokenStore.ReadAsync();
        
        if (state == null || string.IsNullOrEmpty(state.RefreshToken))
        {
            throw new NotAuthorizedException();
        }

        if (state.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(1))
        {
            try
            {
                var options = _options.Value;
                var response = await _httpClient.RefreshTokenAsync(state.RefreshToken, options.ClientId, options.ClientSecret);
                state = MapToTokenState(response);
                await _tokenStore.WriteAsync(state);
            }
            catch (HttpRequestException ex)
            {
                throw new StravaApiException("Failed to refresh access token", ex);
            }
        }

        return state.AccessToken;
    }

    private static TokenState MapToTokenState(StravaTokenResponse response)
    {
        return new TokenState
        {
            AccessToken = response.AccessToken,
            ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(response.ExpiresAt),
            RefreshToken = response.RefreshToken
        };
    }
}
