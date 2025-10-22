using StravaApi.Presentation.Endpoints;

namespace StravaApi.Extensions;

public static class WebApplicationExtensions
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapHealthEndpoints();
        app.MapStravaEndpoints();
    }
}
