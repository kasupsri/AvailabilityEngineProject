using AvailabilityEngineProject.Application.Commands.PutBusy;
using AvailabilityEngineProject.Application.Repository;

namespace AvailabilityEngineProject.Application.Tests.Commands.PutBusy;

public class PutBusyCommandTests
{
    private static TimeInterval I(string start, string end) =>
        new TimeInterval(DateTimeOffset.Parse(start), DateTimeOffset.Parse(end));

    [Fact]
    public async Task ExecuteAsync_CallsRepositoryWithNormalizedIntervals()
    {
        var repo = new Mock<ICalendarCommandRepository>();
        var queryRepo = new Mock<ICalendarQueryRepository>();
        queryRepo.Setup(x => x.GetBusyByEmailsAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IReadOnlyList<TimeInterval>>(StringComparer.OrdinalIgnoreCase));
        var normalizer = new Mock<IBusyNormalizationService>();
        var normalized = new[] { I("2026-02-06T01:00:00Z", "2026-02-06T03:00:00Z") };
        normalizer.Setup(x => x.Normalize(It.IsAny<IEnumerable<TimeInterval>>())).Returns(normalized);
        var command = new PutBusyCommand(repo.Object, queryRepo.Object, normalizer.Object);

        var input = new[] { I("2026-02-06T01:00:00Z", "2026-02-06T02:00:00Z"), I("2026-02-06T02:00:00Z", "2026-02-06T03:00:00Z") };
        var result = await command.ExecuteAsync("alice@example.com", "Alice", input, CancellationToken.None);

        result.Email.Should().Be("alice@example.com");
        result.Name.Should().Be("Alice");
        result.Busy.Should().BeEquivalentTo(normalized);
        repo.Verify(x => x.ReplaceBusyAsync("alice@example.com", "Alice", normalized, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_MergesExistingWithIncomingAndNormalizes()
    {
        var existing = new[] { I("2026-02-05T10:00:00Z", "2026-02-05T11:00:00Z") };
        var queryRepo = new Mock<ICalendarQueryRepository>();
        queryRepo.Setup(x => x.GetBusyByEmailsAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IReadOnlyList<TimeInterval>>(StringComparer.OrdinalIgnoreCase)
            {
                ["alice@example.com"] = existing
            });
        var repo = new Mock<ICalendarCommandRepository>();
        var normalizer = new Mock<IBusyNormalizationService>();
        IReadOnlyList<TimeInterval>? normalizedArg = null;
        normalizer.Setup(x => x.Normalize(It.IsAny<IEnumerable<TimeInterval>>()))
            .Callback<IEnumerable<TimeInterval>>(arg => normalizedArg = arg.ToList())
            .Returns((IEnumerable<TimeInterval> arg) => (IReadOnlyList<TimeInterval>)arg.OrderBy(i => i.Start).ToList());
        var command = new PutBusyCommand(repo.Object, queryRepo.Object, normalizer.Object);

        var incoming = new[] { I("2026-02-06T14:00:00Z", "2026-02-06T15:00:00Z") };
        await command.ExecuteAsync("alice@example.com", "Alice", incoming, CancellationToken.None);

        normalizedArg.Should().NotBeNull();
        normalizedArg!.Count.Should().Be(2);
        normalizedArg.Should().Contain(existing[0]);
        normalizedArg.Should().Contain(incoming[0]);
        repo.Verify(x => x.ReplaceBusyAsync("alice@example.com", "Alice", It.Is<IReadOnlyList<TimeInterval>>(l => l.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullRepository_Throws()
    {
        var queryRepo = new Mock<ICalendarQueryRepository>().Object;
        var normalizer = new Mock<IBusyNormalizationService>().Object;
        var act = () => new PutBusyCommand(null!, queryRepo, normalizer);
        act.Should().Throw<ArgumentNullException>().WithParameterName("commandRepository");
    }

    [Fact]
    public void Constructor_WithNullQueryRepository_Throws()
    {
        var repo = new Mock<ICalendarCommandRepository>().Object;
        var normalizer = new Mock<IBusyNormalizationService>().Object;
        var act = () => new PutBusyCommand(repo, null!, normalizer);
        act.Should().Throw<ArgumentNullException>().WithParameterName("queryRepository");
    }

    [Fact]
    public void Constructor_WithNullNormalizationService_Throws()
    {
        var repo = new Mock<ICalendarCommandRepository>().Object;
        var queryRepo = new Mock<ICalendarQueryRepository>().Object;
        var act = () => new PutBusyCommand(repo, queryRepo, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("normalizationService");
    }
}
