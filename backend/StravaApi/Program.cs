using StravaApi.Extensions;
using StravaApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddPresentationServices();

var app = builder.Build();

// Configure pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseRouting();

// Map endpoints
app.MapEndpoints();

app.Run();
