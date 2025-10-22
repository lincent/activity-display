using Microsoft.AspNetCore.Mvc;

namespace StravaApi.Presentation.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Health")
            .WithOpenApi();

        group.MapGet("/health", GetHealth)
            .WithName("GetHealth")
            .WithSummary("Health check endpoint")
            .Produces<HealthResponse>();
    }

    private static IResult GetHealth()
    {
        return Results.Ok(new HealthResponse("ok", DateTime.UtcNow));
    }
}

public record HealthResponse(string Status, DateTime Timestamp);
