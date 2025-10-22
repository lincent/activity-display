using Microsoft.Extensions.Options;

namespace StravaApi.Infrastructure.Configuration;

public class StravaOptions
{
    public const string SectionName = "Strava";
    
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}

public static class StravaOptionsExtensions
{
    public static StravaOptions GetStravaOptions(this IConfiguration configuration)
    {
        var options = new StravaOptions();
        configuration.GetSection(StravaOptions.SectionName).Bind(options);
        return options;
    }
}
