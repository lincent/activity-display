namespace StravaApi.Application.Exceptions;

public class StravaApiException : Exception
{
    public StravaApiException(string message) : base(message) { }
    public StravaApiException(string message, Exception innerException) : base(message, innerException) { }
}

public class NotAuthorizedException : StravaApiException
{
    public NotAuthorizedException() : base("Not authorized: refresh token missing. Visit /api/strava/auth-url") { }
}

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message) { }
}
