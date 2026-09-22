using System.Text.Json;
using LeanProd.Application.Features.Scheduling;
using Xunit;

namespace LeanProd.Api.IntegrationTests;

public sealed class SchedulingTimeJsonTests
{
    [Theory]
    [InlineData("08:00:00", 8)]
    [InlineData("02:00:00", 2)]
    public void Schedule_interval_accepts_normalized_time(string time, int hour)
    {
        var json = JsonSerializer.Serialize(new { startTime = time, endTime = "16:00:00" });
        var interval = JsonSerializer.Deserialize<WorkScheduleIntervalModel>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(new TimeOnly(hour, 0), interval!.StartTime);
    }

    [Fact]
    public void Browser_time_without_seconds_requires_normalization()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<WorkScheduleIntervalModel>(
            """{"startTime":"08:00","endTime":"16:00"}""",
            new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
