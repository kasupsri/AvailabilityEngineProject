using AvailabilityEngineProject.Domain;

namespace AvailabilityEngineProject.Application.Tests.Domain;

public class TimeIntervalCreateTests
{
    [Fact]
    public void Create_WhenEndAfterStart_ReturnsInterval()
    {
        var start = DateTimeOffset.Parse("2026-02-06T10:00:00Z");
        var end = DateTimeOffset.Parse("2026-02-06T11:00:00Z");

        var interval = TimeInterval.Create(start, end);

        interval.Start.Should().Be(start);
        interval.End.Should().Be(end);
        interval.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenEndEqualsStart_ThrowsArgumentException()
    {
        var t = DateTimeOffset.Parse("2026-02-06T10:00:00Z");

        var act = () => TimeInterval.Create(t, t);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("end")
            .WithMessage("*End must be after start*");
    }

    [Fact]
    public void Create_WhenEndBeforeStart_ThrowsArgumentException()
    {
        var start = DateTimeOffset.Parse("2026-02-06T11:00:00Z");
        var end = DateTimeOffset.Parse("2026-02-06T10:00:00Z");

        var act = () => TimeInterval.Create(start, end);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("end");
    }
}
