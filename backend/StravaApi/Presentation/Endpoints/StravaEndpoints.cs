using Microsoft.AspNetCore.Mvc;
using StravaApi.Application.Services;
using StravaApi.Application.DTOs;
using StravaApi.Application.Exceptions;

namespace StravaApi.Presentation.Endpoints;

public static class StravaEndpoints
{
    public static void MapStravaEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/strava")
            .WithTags("Strava")
            .WithOpenApi();

        group.MapGet("/auth-url", GetAuthUrl)
            .WithName("GetAuthUrl")
            .WithSummary("Get Strava authorization URL")
            .Produces<AuthUrlResponse>();

        group.MapGet("/oauth/callback", HandleOAuthCallback)
            .WithName("HandleOAuthCallback")
            .WithSummary("Handle OAuth callback from Strava")
            .Produces(302)
            .ProducesProblem(400);

        group.MapGet("/activities", GetActivities)
            .WithName("GetActivities")
            .WithSummary("Get latest running activities")
            .Produces<List<RunDto>>()
            .ProducesProblem(401)
            .ProducesProblem(500);
    }

    private static IResult GetAuthUrl(
        [FromServices] IStravaAuthService authService)
    {
        try
        {
            var url = authService.BuildAuthorizeUrl();
            return Results.Ok(new AuthUrlResponse(url));
        }
        catch (ConfigurationException ex)
        {
            return Results.Problem(
                title: "Configuration Error",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> HandleOAuthCallback(
        [FromQuery] string code,
        [FromServices] IStravaAuthService authService)
    {
        if (string.IsNullOrEmpty(code))
        {
            return Results.BadRequest("Authorization code is required");
        }

        try
        {
            await authService.ExchangeCodeAsync(code);
            return Results.Redirect("http://localhost:4200/login?connected=1");
        }
        catch (StravaApiException ex)
        {
            return Results.Problem(
                title: "Strava API Error",
                detail: ex.Message,
                statusCode: 400);
        }
        catch (Exception)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "An unexpected error occurred during authorization",
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetActivities(
        [FromQuery] int? perPage,
        [FromServices] IStravaAuthService authService,
        [FromServices] IStravaApiService apiService)
    {
        try
        {
            var accessToken = await authService.GetValidAccessTokenAsync();
            var activities = await apiService.GetRunningActivitiesAsync(accessToken, perPage ?? 10);
            return Results.Ok(activities);
        }
        catch (NotAuthorizedException)
        {
            return Results.Problem(
                title: "Not Authorized",
                detail: "Please complete Strava authorization first",
                statusCode: 401);
        }
        catch (StravaApiException ex)
        {
            return Results.Problem(
                title: "Strava API Error",
                detail: ex.Message,
                statusCode: 400);
        }
        catch (Exception)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "An unexpected error occurred while fetching activities",
                statusCode: 500);
        }
    }
}

public record AuthUrlResponse(string Url);
