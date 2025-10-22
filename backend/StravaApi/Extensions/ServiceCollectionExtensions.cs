using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using StravaApi.Application.Services;
using StravaApi.Infrastructure.Services;
using StravaApi.Infrastructure.Configuration;

namespace StravaApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IStravaAuthService, StravaAuthService>();
        services.AddScoped<IStravaApiService, StravaApiService>();
        
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StravaOptions>(configuration.GetSection("Strava"));
        services.AddHttpClient();
        services.AddScoped<ITokenStore, FileTokenStore>();
        services.AddScoped<IStravaHttpClient, StravaHttpClient>();
        
        return services;
    }

    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        
        return services;
    }
}
