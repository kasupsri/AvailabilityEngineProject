using AvailabilityEngineProject.Application.Services;

namespace AvailabilityEngineProject.Application.Tests.Services;

public class BusyNormalizationServiceTests
{
    private readonly BusyNormalizationService _service = new();

    private static TimeInterval I(string start, string end) =>
        new TimeInterval(DateTimeOffset.Parse(start), DateTimeOffset.Parse(end));

    [Fact]
    public void Normalize_EmptyInput_ReturnsEmpty()
    {
        var result = _service.Normalize(Array.Empty<TimeInterval>());
        result.Should().BeEmpty();
    }

    [Fact]
    public void Normalize_InvalidIntervals_AreFilteredOut()
    {
        var input = new[]
        {
            I("2026-02-06T10:00:00Z", "2026-02-06T11:00:00Z"),
            new TimeInterval(DateTimeOffset.Parse("2026-02-06T12:00:00Z"), DateTimeOffset.Parse("2026-02-06T12:00:00Z")),
            new TimeInterval(DateTimeOffset.Parse("2026-02-06T14:00:00Z"), DateTimeOffset.Parse("2026-02-06T13:00:00Z")),
        };
        var result = _service.Normalize(input);
        result.Should().HaveCount(1);
        result[0].Start.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T10:00:00Z").UtcDateTime);
        result[0].End.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T11:00:00Z").UtcDateTime);
    }

    [Fact]
    public void Normalize_OverlappingIntervals_Merged()
    {
        var input = new[]
        {
            I("2026-02-06T01:00:00Z", "2026-02-06T02:30:00Z"),
            I("2026-02-06T02:00:00Z", "2026-02-06T03:00:00Z"),
        };
        var result = _service.Normalize(input);
        result.Should().HaveCount(1);
        result[0].Start.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T01:00:00Z").UtcDateTime);
        result[0].End.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T03:00:00Z").UtcDateTime);
    }

    [Fact]
    public void Normalize_TouchingIntervals_KeptSeparate()
    {
        var input = new[]
        {
            I("2026-02-06T01:00:00Z", "2026-02-06T02:00:00Z"),
            I("2026-02-06T02:00:00Z", "2026-02-06T03:00:00Z"),
        };
        var result = _service.Normalize(input);
        result.Should().HaveCount(2);
        result[0].End.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T02:00:00Z").UtcDateTime);
        result[1].Start.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T02:00:00Z").UtcDateTime);
    }

    [Fact]
    public void Normalize_UnsortedInput_SortedAndMerged()
    {
        var input = new[]
        {
            I("2026-02-06T03:00:00Z", "2026-02-06T04:00:00Z"),
            I("2026-02-06T01:00:00Z", "2026-02-06T02:00:00Z"),
            I("2026-02-06T02:30:00Z", "2026-02-06T03:30:00Z"),
        };
        var result = _service.Normalize(input);
        result.Should().HaveCount(2);
        result[0].Start.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T01:00:00Z").UtcDateTime);
        result[0].End.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T02:00:00Z").UtcDateTime);
        result[1].Start.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T02:30:00Z").UtcDateTime);
        result[1].End.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T04:00:00Z").UtcDateTime);
    }

    [Fact]
    public void Normalize_ContainedInterval_MergedIntoOne()
    {
        var input = new[]
        {
            I("2026-02-06T01:00:00Z", "2026-02-06T05:00:00Z"),
            I("2026-02-06T02:00:00Z", "2026-02-06T03:00:00Z"),
        };
        var result = _service.Normalize(input);
        result.Should().HaveCount(1);
        result[0].Start.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T01:00:00Z").UtcDateTime);
        result[0].End.UtcDateTime.Should().Be(DateTimeOffset.Parse("2026-02-06T05:00:00Z").UtcDateTime);
    }
}
