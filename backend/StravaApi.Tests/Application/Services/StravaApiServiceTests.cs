using FluentAssertions;
using Moq;
using StravaApi.Application.DTOs;
using StravaApi.Application.Exceptions;
using StravaApi.Application.Services;
using StravaApi.Infrastructure.Models;

namespace StravaApi.Tests.Application.Services;

public class StravaApiServiceTests
{
    private readonly Mock<IStravaHttpClient> _mockHttpClient;
    private readonly StravaApiService _service;

    public StravaApiServiceTests()
    {
        _mockHttpClient = new Mock<IStravaHttpClient>();
        _service = new StravaApiService(_mockHttpClient.Object);
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_WithValidToken_ReturnsRunActivities()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var stravaActivities = new List<StravaActivity>
        {
            new()
            {
                Id = 1,
                Name = "Morning Run",
                Type = "Run",
                StartDateLocal = "2023-10-01T07:00:00Z",
                Distance = 5000, // 5km in meters
                MovingTime = 1800, // 30 minutes in seconds
                AverageSpeed = 2.78 // ~2.78 m/s (10 km/h)
            },
            new()
            {
                Id = 2,
                Name = "Bike Ride",
                Type = "Ride",
                StartDateLocal = "2023-10-01T08:00:00Z",
                Distance = 10000,
                MovingTime = 1200,
                AverageSpeed = 8.33
            },
            new()
            {
                Id = 3,
                Name = "Evening Run",
                Type = "run", // lowercase to test case insensitive
                StartDateLocal = "2023-10-01T18:00:00Z",
                Distance = 3000,
                MovingTime = 900,
                AverageSpeed = 3.33
            }
        };

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        var result = await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        result.Should().HaveCount(2); // Only runs, not bike ride
        
        var firstRun = result.First();
        firstRun.Id.Should().Be(1);
        firstRun.Name.Should().Be("Morning Run");
        firstRun.DistanceKm.Should().Be(5.0);
        firstRun.MovingTimeSec.Should().Be(1800);
        firstRun.AveragePaceMinPerKm.Should().BeApproximately(6.0, 0.1); // 1000/60 / 2.78 ≈ 6.0 min/km

        var secondRun = result.Last();
        secondRun.Id.Should().Be(3);
        secondRun.Name.Should().Be("Evening Run");
        secondRun.DistanceKm.Should().Be(3.0);
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_WithNoRunActivities_ReturnsEmptyList()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var stravaActivities = new List<StravaActivity>
        {
            new()
            {
                Id = 1,
                Name = "Bike Ride",
                Type = "Ride",
                StartDateLocal = "2023-10-01T08:00:00Z",
                Distance = 10000,
                MovingTime = 1200,
                AverageSpeed = 8.33
            },
            new()
            {
                Id = 2,
                Name = "Swimming",
                Type = "Swim",
                StartDateLocal = "2023-10-01T09:00:00Z",
                Distance = 1000,
                MovingTime = 1800,
                AverageSpeed = 0.56
            }
        };

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        var result = await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_WithHttpRequestException_ThrowsStravaApiException()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var httpException = new HttpRequestException("Network error");

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ThrowsAsync(httpException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<StravaApiException>(() =>
            _service.GetRunningActivitiesAsync(accessToken, perPage));

        exception.Message.Should().Be("Failed to fetch activities from Strava");
        exception.InnerException.Should().Be(httpException);
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_WithZeroAverageSpeed_HandlesGracefully()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var stravaActivities = new List<StravaActivity>
        {
            new()
            {
                Id = 1,
                Name = "Stationary Run",
                Type = "Run",
                StartDateLocal = "2023-10-01T07:00:00Z",
                Distance = 1000,
                MovingTime = 600,
                AverageSpeed = 0 // Zero speed
            }
        };

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        var result = await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        result.Should().HaveCount(1);
        result.First().AveragePaceMinPerKm.Should().Be(0);
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_VerifyPaceCalculation()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var stravaActivities = new List<StravaActivity>
        {
            new()
            {
                Id = 1,
                Name = "Test Run",
                Type = "Run",
                StartDateLocal = "2023-10-01T07:00:00Z",
                Distance = 1000, // 1km
                MovingTime = 300, // 5 minutes
                AverageSpeed = 3.33 // m/s (1km in 300s = 1000/300 = 3.33 m/s)
            }
        };

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        var result = await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        result.Should().HaveCount(1);
        var run = result.First();
        run.DistanceKm.Should().Be(1.0);
        // Pace calculation: (1000/60) / 3.33 = 16.67 / 3.33 ≈ 5.0 min/km
        run.AveragePaceMinPerKm.Should().BeApproximately(5.0, 0.1);
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_VerifyDataMapping()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var stravaActivities = new List<StravaActivity>
        {
            new()
            {
                Id = 12345,
                Name = "Weekend Long Run",
                Type = "Run",
                StartDateLocal = "2023-10-15T06:30:00Z",
                Distance = 21097.5, // Half marathon distance in meters
                MovingTime = 7200, // 2 hours
                AverageSpeed = 2.93 // ~2.93 m/s
            }
        };

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        var result = await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        result.Should().HaveCount(1);
        var run = result.First();
        
        // Verify all mapped properties
        run.Id.Should().Be(12345);
        run.Name.Should().Be("Weekend Long Run");
        run.StartDateLocal.Should().Be("2023-10-15T06:30:00Z");
        run.DistanceKm.Should().Be(21.1); // Rounded to 2 decimal places
        run.MovingTimeSec.Should().Be(7200);
        
        // Pace calculation: (1000/60) / 2.93 ≈ 5.69 min/km
        run.AveragePaceMinPerKm.Should().BeApproximately(5.69, 0.1);
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_WithMixedCaseRunTypes_FiltersCorrectly()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var stravaActivities = new List<StravaActivity>
        {
            new()
            {
                Id = 1,
                Name = "Run 1",
                Type = "Run",
                StartDateLocal = "2023-10-01T07:00:00Z",
                Distance = 5000,
                MovingTime = 1800,
                AverageSpeed = 2.78
            },
            new()
            {
                Id = 2,
                Name = "Run 2",
                Type = "RUN",
                StartDateLocal = "2023-10-01T08:00:00Z",
                Distance = 3000,
                MovingTime = 1080,
                AverageSpeed = 2.78
            },
            new()
            {
                Id = 3,
                Name = "Run 3",
                Type = "run",
                StartDateLocal = "2023-10-01T09:00:00Z",
                Distance = 4000,
                MovingTime = 1440,
                AverageSpeed = 2.78
            },
            new()
            {
                Id = 4,
                Name = "Bike Ride",
                Type = "Ride",
                StartDateLocal = "2023-10-01T10:00:00Z",
                Distance = 10000,
                MovingTime = 1200,
                AverageSpeed = 8.33
            }
        };

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        var result = await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        result.Should().HaveCount(3); // All three runs should be included
        result.Select(r => r.Id).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_WithEmptyActivitiesList_ReturnsEmptyList()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var stravaActivities = new List<StravaActivity>();

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        var result = await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_VerifyRounding()
    {
        // Arrange
        var accessToken = "valid_token";
        var perPage = 10;
        var stravaActivities = new List<StravaActivity>
        {
            new()
            {
                Id = 1,
                Name = "Precision Test Run",
                Type = "Run",
                StartDateLocal = "2023-10-01T07:00:00Z",
                Distance = 5555.555, // Should round to 5.56 km
                MovingTime = 1800,
                AverageSpeed = 2.777777 // Should result in pace rounded to 2 decimal places
            }
        };

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        var result = await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        result.Should().HaveCount(1);
        var run = result.First();
        run.DistanceKm.Should().Be(5.56); // Verify distance rounding
        run.AveragePaceMinPerKm.Should().BeGreaterThan(0); // Verify pace is calculated and positive
    }

    [Fact]
    public async Task GetRunningActivitiesAsync_CallsHttpClientWithCorrectParameters()
    {
        // Arrange
        var accessToken = "test_access_token";
        var perPage = 25;
        var stravaActivities = new List<StravaActivity>();

        _mockHttpClient
            .Setup(x => x.GetActivitiesAsync(accessToken, perPage))
            .ReturnsAsync(stravaActivities);

        // Act
        await _service.GetRunningActivitiesAsync(accessToken, perPage);

        // Assert
        _mockHttpClient.Verify(
            x => x.GetActivitiesAsync(accessToken, perPage),
            Times.Once);
    }
}