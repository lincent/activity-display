using StravaApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
  options.AddPolicy("frontend", policy =>
  {
    policy.WithOrigins("http://localhost:4200")
      .AllowAnyHeader()
      .AllowAnyMethod();
  });
});

// Config binding
builder.Services.Configure<StravaOptions>(builder.Configuration.GetSection("Strava"));

builder.Services.AddHttpClient();

// App services
builder.Services.AddSingleton<ITokenStore, FileTokenStore>();
builder.Services.AddSingleton<IStravaAuthService, StravaAuthService>();
builder.Services.AddSingleton<IStravaApiClient, StravaApiClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseCors("frontend");

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

// Strava endpoints
app.MapGet("/api/strava/auth-url", (IStravaAuthService auth) => Results.Ok(new { url = auth.BuildAuthorizeUrl() }));

app.MapGet("/api/strava/oauth/callback", async (string code, IStravaAuthService auth) =>
{
  await auth.ExchangeCodeAsync(code);
  return Results.Redirect("http://localhost:4200/setup?connected=1");
});

app.MapGet("/api/strava/activities", async (int? perPage, IStravaAuthService auth, IStravaApiClient client) =>
{
  var accessToken = await auth.GetValidAccessTokenAsync();
  var activities = await client.GetAthleteActivitiesAsync(accessToken, perPage ?? 10);
  var runs = activities.Where(a => string.Equals(a.Type, "Run", StringComparison.OrdinalIgnoreCase))
    .Select(ActivityMapper.ToRunDto);
  return Results.Ok(runs);
});

app.Run();
