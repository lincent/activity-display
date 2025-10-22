using System.Security.Cryptography;
using System.Text.Json;
using StravaApi.Application.Services;
using StravaApi.Application.DTOs;

namespace StravaApi.Infrastructure.Services;

public class FileTokenStore : ITokenStore
{
    private readonly string _filePath;

    public FileTokenStore()
    {
        var appData = Path.Combine(AppContext.BaseDirectory, "AppData");
        Directory.CreateDirectory(appData);
        _filePath = Path.Combine(appData, "strava_tokens.json");
    }

    public async Task<TokenState?> ReadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);
            var persisted = JsonSerializer.Deserialize<PersistedTokenState>(json) ?? new PersistedTokenState();
            
            if (string.IsNullOrEmpty(persisted.RefreshTokenProtected))
            {
                return null;
            }

            var refreshToken = Unprotect(persisted.RefreshTokenProtected);
            
            return new TokenState
            {
                AccessToken = persisted.AccessToken,
                ExpiresAt = persisted.ExpiresAt,
                RefreshToken = refreshToken
            };
        }
        catch (Exception)
        {
            // If we can't read the file, treat as not authorized
            return null;
        }
    }

    public async Task WriteAsync(TokenState state)
    {
        var protectedRefresh = Protect(state.RefreshToken);
        var persisted = new PersistedTokenState
        {
            AccessToken = state.AccessToken,
            ExpiresAt = state.ExpiresAt,
            RefreshTokenProtected = protectedRefresh
        };

        var json = JsonSerializer.Serialize(persisted, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }

    private static string Protect(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var data = System.Text.Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    private static string Unprotect(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        try
        {
            var data = Convert.FromBase64String(value);
            var unprotected = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return System.Text.Encoding.UTF8.GetString(unprotected);
        }
        catch
        {
            return string.Empty;
        }
    }

    private class PersistedTokenState
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public string RefreshTokenProtected { get; set; } = string.Empty;
    }
}
