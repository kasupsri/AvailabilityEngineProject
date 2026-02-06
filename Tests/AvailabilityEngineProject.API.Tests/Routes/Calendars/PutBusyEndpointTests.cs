using AvailabilityEngineProject.API.Routes.Calendars.Endpoints;
using AvailabilityEngineProject.API.Routes.Calendars.Models;
using AvailabilityEngineProject.Application.Commands.PutBusy;

namespace AvailabilityEngineProject.API.Tests.Routes.Calendars;

public class PutBusyEndpointTests
{
    [Fact]
    public async Task Handle_WithValidRequest_ReturnsOkWithNormalizedBusy()
    {
        var request = new BusyPutRequest("Alice Smith", new[]
        {
            new BusyIntervalDto("2026-02-06T01:00:00Z", "2026-02-06T03:00:00Z"),
        });
        var expected = new PutBusyResult("alice@example.com", "Alice Smith", new[] { new AvailabilityEngineProject.Domain.TimeInterval(DateTimeOffset.Parse("2026-02-06T01:00:00Z"), DateTimeOffset.Parse("2026-02-06T03:00:00Z")) });
        var command = new Mock<IPutBusyCommand>();
        command.Setup(x => x.ExecuteAsync("alice@example.com", "Alice Smith", It.IsAny<IReadOnlyList<AvailabilityEngineProject.Domain.TimeInterval>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await PutBusy.Handle("alice@example.com", request, command.Object, CancellationToken.None);

        result.Should().NotBeNull();
        var ok = result as Microsoft.AspNetCore.Http.HttpResults.Ok<BusyPutResponse>;
        ok.Should().NotBeNull();
        ok!.Value!.Email.Should().Be("alice@example.com");
        ok.Value.Name.Should().Be("Alice Smith");
        ok.Value.Busy.Should().HaveCount(1);
        ok.Value.Busy[0].Start.Should().Contain("2026-02-06");
    }

    [Fact]
    public async Task Handle_WithEmptyEmail_ReturnsBadRequest()
    {
        var request = new BusyPutRequest("Alice", Array.Empty<BusyIntervalDto>());
        var command = new Mock<IPutBusyCommand>();

        var result = await PutBusy.Handle("", request, command.Object, CancellationToken.None);

        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        badRequest.Should().NotBeNull();
        command.Verify(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AvailabilityEngineProject.Domain.TimeInterval>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsBadRequest()
    {
        var request = new BusyPutRequest("", Array.Empty<BusyIntervalDto>());
        var command = new Mock<IPutBusyCommand>();

        var result = await PutBusy.Handle("alice@example.com", request, command.Object, CancellationToken.None);

        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        badRequest.Should().NotBeNull();
        command.Verify(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AvailabilityEngineProject.Domain.TimeInterval>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidInterval_ReturnsBadRequest()
    {
        var request = new BusyPutRequest("Alice", new[]
        {
            new BusyIntervalDto("2026-02-06T02:00:00Z", "2026-02-06T01:00:00Z"),
        });
        var command = new Mock<IPutBusyCommand>();

        var result = await PutBusy.Handle("alice@example.com", request, command.Object, CancellationToken.None);

        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        badRequest.Should().NotBeNull();
        command.Verify(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<AvailabilityEngineProject.Domain.TimeInterval>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
