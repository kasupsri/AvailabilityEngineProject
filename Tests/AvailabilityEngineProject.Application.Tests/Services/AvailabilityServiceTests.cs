using AvailabilityEngineProject.Application.Services;

namespace AvailabilityEngineProject.Application.Tests.Services;

public class AvailabilityServiceTests
{
    private static TimeInterval I(string start, string end) =>
        new TimeInterval(DateTimeOffset.Parse(start), DateTimeOffset.Parse(end));

    private static AvailabilityService CreateServiceWithBusyBlocks(params TimeInterval[] blocks)
    {
        var stream = new Mock<IBusyBlockStream>();
        stream.Setup(x => x.GetBusyBlocksAsync(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(blocks);
        return new AvailabilityService(stream.Object);
    }

    [Fact]
    public async Task FindEarliestSlot_NoBusy_ReturnsWindowStart()
    {
        var svc = CreateServiceWithBusyBlocks();
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var slot = await svc.FindEarliestSlotAsync(new[] { "alice" }, ws, we, TimeSpan.FromMinutes(30), CancellationToken.None);
        slot.Should().NotBeNull();
        slot!.Start.Should().Be(ws);
        slot.End.Should().Be(ws + TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task FindEarliestSlot_FullyBusy_ReturnsNull()
    {
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var blocks = new[] { I("2026-02-06T09:00:00Z", "2026-02-06T17:00:00Z") };
        var svc = CreateServiceWithBusyBlocks(blocks);
        var slot = await svc.FindEarliestSlotAsync(new[] { "alice" }, ws, we, TimeSpan.FromMinutes(30), CancellationToken.None);
        slot.Should().BeNull();
    }

    [Fact]
    public async Task FindEarliestSlot_GapExactlyDuration_ReturnsSlot()
    {
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var blocks = new[]
        {
            I("2026-02-06T09:00:00Z", "2026-02-06T10:00:00Z"),
            I("2026-02-06T10:30:00Z", "2026-02-06T17:00:00Z"),
        };
        var svc = CreateServiceWithBusyBlocks(blocks);
        var slot = await svc.FindEarliestSlotAsync(new[] { "alice" }, ws, we, TimeSpan.FromMinutes(30), CancellationToken.None);
        slot.Should().NotBeNull();
        slot!.Start.Should().Be(DateTimeOffset.Parse("2026-02-06T10:00:00Z"));
        slot.End.Should().Be(DateTimeOffset.Parse("2026-02-06T10:30:00Z"));
    }

    [Fact]
    public async Task FindEarliestSlot_InvalidWindow_ReturnsNull()
    {
        var svc = CreateServiceWithBusyBlocks();
        var we = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var ws = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var slot = await svc.FindEarliestSlotAsync(new[] { "alice" }, ws, we, TimeSpan.FromMinutes(30), CancellationToken.None);
        slot.Should().BeNull();
    }

    [Fact]
    public async Task FindEarliestSlot_DurationExceedsWindow_ReturnsNull()
    {
        var svc = CreateServiceWithBusyBlocks();
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T09:15:00Z");
        var slot = await svc.FindEarliestSlotAsync(new[] { "alice" }, ws, we, TimeSpan.FromMinutes(30), CancellationToken.None);
        slot.Should().BeNull();
    }
}
