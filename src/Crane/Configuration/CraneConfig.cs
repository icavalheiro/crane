using Crane.Configuration.Models;

namespace Crane.Configuration;

public sealed class CraneConfig
{
    private const string Prefix = "CRANE_";
    private const string PathSuffix = "_PATH";
    private const string FileNameSuffix = "_FILE_NAME";

    public IReadOnlyList<ComposeConfiguration> ComposeFiles { get; }

    public CraneConfig()
    {
        var environmentVariables = Environment.GetEnvironmentVariables();
        var entries = new List<ComposeConfiguration>();

        foreach (var keyObject in environmentVariables.Keys)
        {
            if (keyObject is not string key)
                continue;

            if (!key.StartsWith(Prefix, StringComparison.Ordinal) || !key.EndsWith(PathSuffix, StringComparison.Ordinal))
                continue;

            var serviceName = key[Prefix.Length..^PathSuffix.Length];
            if (string.IsNullOrWhiteSpace(serviceName))
                continue;

            var path = environmentVariables[key]?.ToString();
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException($"Environment variable '{key}' must define a path.");

            var fileNameKey = $"{Prefix}{serviceName}{FileNameSuffix}";
            var fileName = environmentVariables[fileNameKey]?.ToString();

            entries.Add(new ComposeConfiguration
            {
                Service = serviceName,
                Path = path,
                FileName = string.IsNullOrWhiteSpace(fileName) ? null : fileName
            });
        }

        if (entries.Count == 0)
            throw new InvalidOperationException("No compose configurations found in environment variables.");

        ComposeFiles = [.. entries.OrderBy(entry => entry.Service, StringComparer.Ordinal)];
    }
}