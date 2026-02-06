using AvailabilityEngineProject.Application.Queries.GetBusyInWindow;

namespace AvailabilityEngineProject.Application.Tests.Queries.GetBusyInWindow;

public class GetBusyInWindowHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_EmptyAttendees_ReturnsEmptyResult()
    {
        var repo = new Mock<ICalendarQueryRepository>();
        var handler = new GetBusyInWindowHandler(repo.Object);
        var request = new GetBusyInWindowRequest(
            Array.Empty<string>(),
            DateTimeOffset.Parse("2026-02-06T09:00:00Z"),
            DateTimeOffset.Parse("2026-02-06T17:00:00Z"));

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Attendees.Should().BeEmpty();
        repo.Verify(x => x.GetPersonsAsync(It.IsAny<CancellationToken>()), Times.Never);
        repo.Verify(x => x.GetBusyByEmailsAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_OneAttendee_NoBusy_ReturnsAttendeeWithEmptyBusy()
    {
        var repo = new Mock<ICalendarQueryRepository>();
        var persons = new List<Person> { new Person(1, "a@x.com", "Alice") };
        repo.Setup(x => x.GetPersonsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(persons);
        repo.Setup(x => x.GetBusyByEmailsAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IReadOnlyList<TimeInterval>>(StringComparer.OrdinalIgnoreCase) { ["a@x.com"] = Array.Empty<TimeInterval>() });
        var handler = new GetBusyInWindowHandler(repo.Object);
        var request = new GetBusyInWindowRequest(
            new[] { "a@x.com" },
            DateTimeOffset.Parse("2026-02-06T09:00:00Z"),
            DateTimeOffset.Parse("2026-02-06T17:00:00Z"));

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Attendees.Should().HaveCount(1);
        result.Attendees[0].Email.Should().Be("a@x.com");
        result.Attendees[0].Name.Should().Be("Alice");
        result.Attendees[0].Busy.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_OneAttendee_WithBusy_ClipsToWindow()
    {
        var repo = new Mock<ICalendarQueryRepository>();
        var persons = new List<Person> { new Person(1, "a@x.com", "Alice") };
        var busy = new List<TimeInterval>
        {
            new TimeInterval(DateTimeOffset.Parse("2026-02-06T08:00:00Z"), DateTimeOffset.Parse("2026-02-06T12:00:00Z"))
        };
        repo.Setup(x => x.GetPersonsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(persons);
        repo.Setup(x => x.GetBusyByEmailsAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, IReadOnlyList<TimeInterval>>(StringComparer.OrdinalIgnoreCase) { ["a@x.com"] = busy });
        var handler = new GetBusyInWindowHandler(repo.Object);
        var ws = DateTimeOffset.Parse("2026-02-06T09:00:00Z");
        var we = DateTimeOffset.Parse("2026-02-06T17:00:00Z");
        var request = new GetBusyInWindowRequest(new[] { "a@x.com" }, ws, we);

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        result.Attendees.Should().HaveCount(1);
        result.Attendees[0].Busy.Should().HaveCount(1);
        result.Attendees[0].Busy[0].Start.Should().Be(ws);
        result.Attendees[0].Busy[0].End.Should().Be(DateTimeOffset.Parse("2026-02-06T12:00:00Z"));
    }
}
