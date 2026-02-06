namespace Kasupsri.Utilities.Logging.Correlation;

public interface ICorrelationIdAccessor
{
    string? GetCorrelationId();
    void SetCorrelationId(string correlationId);
}
