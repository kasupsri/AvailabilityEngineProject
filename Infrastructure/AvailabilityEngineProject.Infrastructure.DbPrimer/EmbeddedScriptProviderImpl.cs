using System.Reflection;
using System.Text;

namespace AvailabilityEngineProject.Infrastructure.DbPrimer;

public class EmbeddedScriptProviderImpl
{
    private readonly Assembly _assembly;

    public EmbeddedScriptProviderImpl()
    {
        _assembly = Assembly.GetExecutingAssembly();
    }

    public IEnumerable<string> GetScripts()
    {
        var resourceNames = _assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name);

        foreach (var resourceName in resourceNames)
        {
            using var stream = _assembly.GetManifestResourceStream(resourceName);
            if (stream == null) continue;

            using var reader = new StreamReader(stream, Encoding.UTF8);
            yield return reader.ReadToEnd();
        }
    }
}
