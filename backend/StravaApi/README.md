# Strava API Backend - Refactored Architecture

## Overview
A clean, layered .NET 8 minimal API that handles Strava OAuth authentication and provides running activities data.

## Architecture

### Layers
- **Presentation**: Endpoints, middleware, and API contracts
- **Application**: Business logic, services, and DTOs
- **Infrastructure**: External API clients, token storage, and configuration

### Key Improvements
- ✅ **Separation of Concerns**: Clear layer boundaries with proper abstractions
- ✅ **Dependency Injection**: Proper DI container configuration
- ✅ **Error Handling**: Centralized exception handling with proper HTTP status codes
- ✅ **Configuration Management**: Strongly-typed configuration with validation
- ✅ **Security**: Encrypted token storage using Windows DPAPI
- ✅ **API Documentation**: OpenAPI/Swagger integration
- ✅ **Logging**: Structured logging throughout the application

## Project Structure

```
backend/StravaApi/
├── Application/
│   ├── DTOs/                 # Data transfer objects
│   ├── Exceptions/           # Custom exception types
│   └── Services/             # Business logic services
├── Infrastructure/
│   ├── Configuration/        # Configuration models
│   ├── Models/              # External API models
│   └── Services/            # Infrastructure services
├── Presentation/
│   └── Endpoints/           # API endpoints
├── Extensions/              # Service registration extensions
├── Middleware/              # Custom middleware
└── Program.cs              # Application entry point
```

## Services

### Application Services
- `IStravaAuthService`: Handles OAuth flow and token management
- `IStravaApiService`: Business logic for activities

### Infrastructure Services
- `ITokenStore`: Secure token persistence
- `IStravaHttpClient`: HTTP client for Strava API

## Configuration

### User Secrets (Development)
```bash
dotnet user-secrets set "Strava:ClientId" "<YOUR_CLIENT_ID>"
dotnet user-secrets set "Strava:ClientSecret" "<YOUR_CLIENT_SECRET>"
```

### Environment Variables (Production)
```bash
Strava__ClientId=<YOUR_CLIENT_ID>
Strava__ClientSecret=<YOUR_CLIENT_SECRET>
Strava__RedirectUri=<YOUR_REDIRECT_URI>
```

## API Endpoints

- `GET /api/health` - Health check
- `GET /api/strava/auth-url` - Get Strava authorization URL
- `GET /api/strava/oauth/callback` - Handle OAuth callback
- `GET /api/strava/activities` - Get latest running activities

## Error Handling

The application uses a centralized exception handling middleware that:
- Maps custom exceptions to appropriate HTTP status codes
- Provides consistent error response format
- Logs all unhandled exceptions
- Prevents sensitive information leakage

## Security Features

- **Token Encryption**: Refresh tokens encrypted using Windows DPAPI
- **Configuration Security**: Secrets stored in User Secrets (dev) or environment variables (prod)
- **CORS**: Configured for frontend origin only
- **Input Validation**: Query parameter validation

## Running the Application

```bash
# Development
dotnet run --project backend/StravaApi

# Production
dotnet run --project backend/StravaApi --environment Production
```

## Testing

The application is designed for easy unit testing with:
- Interface-based dependencies
- Dependency injection
- Separated concerns
- Mockable external dependencies
