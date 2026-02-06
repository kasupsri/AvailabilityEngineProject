using AvailabilityEngineProject.API.Routes.Availability.Endpoints;
using AvailabilityEngineProject.Application.Queries.GetAvailability;

namespace AvailabilityEngineProject.API.Tests.Routes.Availability;

public class GetAvailabilityEndpointTests
{
    [Fact]
    public async Task Handle_WithValidQuery_ReturnsOkWithFoundSlot()
    {
        var query = new Mock<IGetAvailabilityQuery>();
        var slot = new AvailabilityEngineProject.Domain.TimeInterval(
            DateTimeOffset.Parse("2026-02-06T10:00:00Z"),
            DateTimeOffset.Parse("2026-02-06T10:30:00Z"));
        query.Setup(x => x.ExecuteAsync(It.IsAny<GetAvailabilityRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAvailabilityResult(true, slot));

        var result = await GetAvailability.Handle(
            "alice,bob",
            "2026-02-06T09:00:00Z",
            "2026-02-06T17:00:00Z",
            30,
            query.Object,
            CancellationToken.None);

        result.Should().NotBeNull();
        var ok = result as Microsoft.AspNetCore.Http.HttpResults.Ok<AvailabilityEngineProject.API.Routes.Availability.Models.AvailabilityResponse>;
        ok.Should().NotBeNull();
        ok!.Value!.Found.Should().BeTrue();
        ok.Value.Start.Should().NotBeNullOrEmpty();
        ok.Value.End.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithMissingAttendees_ReturnsBadRequest()
    {
        var query = new Mock<IGetAvailabilityQuery>();

        var result = await GetAvailability.Handle(
            "",
            "2026-02-06T09:00:00Z",
            "2026-02-06T17:00:00Z",
            30,
            query.Object,
            CancellationToken.None);

        var badRequest = result as Microsoft.AspNetCore.Http.HttpResults.BadRequest<string>;
        badRequest.Should().NotBeNull();
        query.Verify(x => x.ExecuteAsync(It.IsAny<GetAvailabilityRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoSlot_ReturnsOkWithFoundFalse()
    {
        var query = new Mock<IGetAvailabilityQuery>();
        query.Setup(x => x.ExecuteAsync(It.IsAny<GetAvailabilityRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAvailabilityResult(false, null));

        var result = await GetAvailability.Handle(
            "alice,bob",
            "2026-02-06T09:00:00Z",
            "2026-02-06T17:00:00Z",
            30,
            query.Object,
            CancellationToken.None);

        var ok = result as Microsoft.AspNetCore.Http.HttpResults.Ok<AvailabilityEngineProject.API.Routes.Availability.Models.AvailabilityResponse>;
        ok.Should().NotBeNull();
        ok!.Value!.Found.Should().BeFalse();
    }
}
