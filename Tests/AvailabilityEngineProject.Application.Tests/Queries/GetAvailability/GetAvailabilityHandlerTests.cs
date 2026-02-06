using AvailabilityEngineProject.Application.Queries.GetAvailability;
using AvailabilityEngineProject.Application.Services;

namespace AvailabilityEngineProject.Application.Tests.Queries.GetAvailability;

public class GetAvailabilityHandlerTests
{
    private static IClock CreateClock(DateTimeOffset now)
    {
        var clock = new Mock<IClock>();
        clock.Setup(c => c.GetUtcNow()).Returns(now);
        return clock.Object;
    }

    [Fact]
    public async Task ExecuteAsync_NoAttendees_ReturnsSlotAtWindowStart_WhenWindowFitsDuration()
    {
        var availabilityService = new Mock<IAvailabilityService>();
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var clock = CreateClock(DateTimeOffset.Parse("2026-02-06T08:00:00Z"));
        var handler = new GetAvailabilityHandler(availabilityService.Object, clock);
        var request = new GetAvailabilityRequest(Array.Empty<string>(), ws, we, 30);

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Found.Should().BeTrue();
        result.Slot.Should().NotBeNull();
        result.Slot!.Start.Should().Be(ws);
        result.Slot.End.Should().Be(ws + TimeSpan.FromMinutes(30));
        availabilityService.Verify(x => x.FindEarliestSlotAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_NoAttendees_WhenWindowStartInPast_ReturnsSlotAtNow()
    {
        var availabilityService = new Mock<IAvailabilityService>();
        var ws = DateTimeOffset.Parse("2026-02-06T11:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var now = DateTimeOffset.Parse("2026-02-06T12:00:00Z");
        var clock = CreateClock(now);
        var handler = new GetAvailabilityHandler(availabilityService.Object, clock);
        var request = new GetAvailabilityRequest(Array.Empty<string>(), ws, we, 30);

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Found.Should().BeTrue();
        result.Slot.Should().NotBeNull();
        result.Slot!.Start.Should().Be(now);
        result.Slot.End.Should().Be(now + TimeSpan.FromMinutes(30));
        availabilityService.Verify(x => x.FindEarliestSlotAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_NoAttendees_WhenWindowEntirelyInPast_ReturnsNotFound()
    {
        var availabilityService = new Mock<IAvailabilityService>();
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T11:00:00Z");
        var now = DateTimeOffset.Parse("2026-02-06T12:00:00Z");
        var clock = CreateClock(now);
        var handler = new GetAvailabilityHandler(availabilityService.Object, clock);
        var request = new GetAvailabilityRequest(Array.Empty<string>(), ws, we, 30);

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Found.Should().BeFalse();
        result.Slot.Should().BeNull();
        availabilityService.Verify(x => x.FindEarliestSlotAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithAttendees_DelegatesToAvailabilityService()
    {
        var availabilityService = new Mock<IAvailabilityService>();
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var expectedSlot = new TimeInterval(ws, ws + TimeSpan.FromMinutes(30));
        availabilityService.Setup(x => x.FindEarliestSlotAsync(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSlot);
        var clock = CreateClock(DateTimeOffset.Parse("2026-02-06T08:00:00Z"));
        var handler = new GetAvailabilityHandler(availabilityService.Object, clock);
        var request = new GetAvailabilityRequest(new[] { "alice", "bob" }, ws, we, 30);

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Found.Should().BeTrue();
        result.Slot.Should().Be(expectedSlot);
        availabilityService.Verify(x => x.FindEarliestSlotAsync(
            new[] { "alice", "bob" }, ws, we, TimeSpan.FromMinutes(30), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithAttendees_WhenWindowStartInPast_CallsServiceWithEffectiveStartAtNow()
    {
        var availabilityService = new Mock<IAvailabilityService>();
        var ws = DateTimeOffset.Parse("2026-02-06T11:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var now = DateTimeOffset.Parse("2026-02-06T12:00:00Z");
        var expectedSlot = new TimeInterval(now, now + TimeSpan.FromMinutes(30));
        availabilityService.Setup(x => x.FindEarliestSlotAsync(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSlot);
        var clock = CreateClock(now);
        var handler = new GetAvailabilityHandler(availabilityService.Object, clock);
        var request = new GetAvailabilityRequest(new[] { "alice", "bob" }, ws, we, 30);

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Found.Should().BeTrue();
        result.Slot!.Start.Should().Be(now);
        availabilityService.Verify(x => x.FindEarliestSlotAsync(
            new[] { "alice", "bob" }, now, we, TimeSpan.FromMinutes(30), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoSlot_ReturnsNotFound()
    {
        var availabilityService = new Mock<IAvailabilityService>();
        availabilityService.Setup(x => x.FindEarliestSlotAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeInterval?)null);
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var clock = CreateClock(DateTimeOffset.Parse("2026-02-06T08:00:00Z"));
        var handler = new GetAvailabilityHandler(availabilityService.Object, clock);
        var request = new GetAvailabilityRequest(new[] { "alice" }, ws, we, 30);

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Found.Should().BeFalse();
        result.Slot.Should().BeNull();
    }
}
