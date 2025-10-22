using System.Security.Cryptography;
using System.Text.Json;

namespace StravaApi;

public interface ITokenStore
{
  Task<AccessTokenState?> ReadAsync();
  Task WriteAsync(AccessTokenState state);
}

public class FileTokenStore : ITokenStore
{
  private readonly string _filePath;
  public FileTokenStore()
  {
    var appData = Path.Combine(AppContext.BaseDirectory, "AppData");
    Directory.CreateDirectory(appData);
    _filePath = Path.Combine(appData, "strava_tokens.json");
  }

  public async Task<AccessTokenState?> ReadAsync()
  {
    if (!File.Exists(_filePath)) return null;
    var json = await File.ReadAllTextAsync(_filePath);
    var persisted = JsonSerializer.Deserialize<PersistedState>(json) ?? new PersistedState();
    var refresh = Unprotect(persisted.RefreshTokenProtected);
    return new AccessTokenState
    {
      AccessToken = persisted.AccessToken,
      ExpiresAt = persisted.ExpiresAt,
      RefreshToken = refresh
    };
  }

  public async Task WriteAsync(AccessTokenState state)
  {
    var protectedRefresh = Protect(state.RefreshToken);
    var persisted = new PersistedState
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
    var data = System.Text.Encoding.UTF8.GetBytes(value);
    var protectedBytes = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
    return Convert.ToBase64String(protectedBytes);
  }

  private static string Unprotect(string value)
  {
    if (string.IsNullOrEmpty(value)) return string.Empty;
    var data = Convert.FromBase64String(value);
    var unprotected = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
    return System.Text.Encoding.UTF8.GetString(unprotected);
  }

  private class PersistedState
  {
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string RefreshTokenProtected { get; set; } = string.Empty;
  }
}



