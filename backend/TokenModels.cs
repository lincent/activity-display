using System.Text.Json.Serialization;

namespace StravaApi;

public class TokenEnvelope
{
  [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
  [JsonPropertyName("expires_at")] public long ExpiresAt { get; set; }
  [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
  [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
  [JsonPropertyName("token_type")] public string TokenType { get; set; } = string.Empty;
}

public class AccessTokenState
{
  public string AccessToken { get; set; } = string.Empty;
  public DateTimeOffset ExpiresAt { get; set; }
  public string RefreshToken { get; set; } = string.Empty;
}


