using System.Threading;

namespace Kasupsri.Utilities.Logging.Correlation;

public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> _correlationId = new();

    public string? GetCorrelationId()
    {
        return _correlationId.Value;
    }

    public void SetCorrelationId(string correlationId)
    {
        _correlationId.Value = correlationId;
    }
}
