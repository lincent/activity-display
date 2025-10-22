using StravaApi.Application.Services;
using StravaApi.Application.DTOs;
using StravaApi.Application.Exceptions;
using StravaApi.Infrastructure.Models;

namespace StravaApi.Application.Services;

public class StravaApiService : IStravaApiService
{
    private readonly IStravaHttpClient _httpClient;

    public StravaApiService(IStravaHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RunDto>> GetRunningActivitiesAsync(string accessToken, int perPage)
    {
        try
        {
            var activities = await _httpClient.GetActivitiesAsync(accessToken, perPage);
            var runs = activities
                .Where(a => string.Equals(a.Type, "Run", StringComparison.OrdinalIgnoreCase))
                .Select(MapToRunDto)
                .ToList();
            
            return runs;
        }
        catch (HttpRequestException ex)
        {
            throw new StravaApiException("Failed to fetch activities from Strava", ex);
        }
    }

    private static RunDto MapToRunDto(StravaActivity activity)
    {
        var distanceKm = activity.Distance / 1000d;
        var paceMinPerKm = activity.AverageSpeed > 0 ? (1000d / 60d) / activity.AverageSpeed : 0;
        
        return new RunDto
        {
            Id = activity.Id,
            Name = activity.Name,
            StartDateLocal = activity.StartDateLocal,
            DistanceKm = Math.Round(distanceKm, 2),
            MovingTimeSec = activity.MovingTime,
            AveragePaceMinPerKm = Math.Round(paceMinPerKm, 2)
        };
    }
}
