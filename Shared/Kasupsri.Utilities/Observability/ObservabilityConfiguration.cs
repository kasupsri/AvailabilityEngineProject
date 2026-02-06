namespace Kasupsri.Utilities.Observability;

public class ObservabilityConfiguration
{
    public bool Enabled { get; set; } = false;
    public string? Sampler { get; set; }
}
