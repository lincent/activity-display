namespace StravaApi.Application.DTOs;

public class RunDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartDateLocal { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public int MovingTimeSec { get; set; }
    public double AveragePaceMinPerKm { get; set; }
}

public class TokenState
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
