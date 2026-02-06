namespace AvailabilityEngineProject.Application.Queries.GetBusyInWindow;

public interface IGetBusyInWindowQuery
{
    Task<GetBusyInWindowResult> ExecuteAsync(GetBusyInWindowRequest request, CancellationToken cancellationToken);
}
