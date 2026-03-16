using Crane.Configuration.Models;
using System.Text.RegularExpressions;

namespace Crane.Configuration;

public sealed partial class CraneConfig
{
    private const string Prefix = "CRANE_";
    private const string PathSuffix = "_PATH";
    private const string FileNameSuffix = "_FILE_NAME";
    private const string TimerSuffix = "_TIMER";
    private static readonly TimeSpan DefaultTimer = TimeSpan.FromMinutes(5);

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
            var timerKey = $"{Prefix}{serviceName}{TimerSuffix}";
            var timerValue = environmentVariables[timerKey]?.ToString();

            entries.Add(new ComposeConfiguration
            {
                Service = serviceName,
                Path = path,
                FileName = string.IsNullOrWhiteSpace(fileName) ? null : fileName,
                Timer = ParseTimer(timerValue, timerKey)
            });
        }

        if (entries.Count == 0)
            throw new InvalidOperationException("No compose configurations found in environment variables.");

        ComposeFiles = [.. entries.OrderBy(entry => entry.Service, StringComparer.Ordinal)];
    }

    private static TimeSpan ParseTimer(string? rawValue, string variableName)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return DefaultTimer;

        if (TryParseTimer(rawValue, out var parsed))
            return parsed;

        throw new InvalidOperationException(
            $"Environment variable '{variableName}' has an invalid timer '{rawValue}'. Use values like '5m', '5 minutes', '1h', '30s' or '2d'.");
    }

    private static bool TryParseTimer(string value, out TimeSpan parsed)
    {
        var trimmed = value.Trim();

        if (TimeSpan.TryParse(trimmed, out parsed) && parsed > TimeSpan.Zero)
            return true;

        var match = TimerAmountRegex().Match(trimmed);
        if (!match.Success)
        {
            parsed = default;
            return false;
        }

        var amount = int.Parse(match.Groups[1].Value);
        if (amount <= 0)
        {
            parsed = default;
            return false;
        }

        var unit = match.Groups[2].Value.ToLowerInvariant();
        parsed = unit switch
        {
            "s" or "sec" or "secs" or "second" or "seconds" => TimeSpan.FromSeconds(amount),
            "m" or "min" or "mins" or "minute" or "minutes" => TimeSpan.FromMinutes(amount),
            "h" or "hr" or "hrs" or "hour" or "hours" => TimeSpan.FromHours(amount),
            "d" or "day" or "days" => TimeSpan.FromDays(amount),
            _ => default
        };

        return parsed > TimeSpan.Zero;
    }

    [GeneratedRegex("^(\\d+)\\s*([a-zA-Z]+)$", RegexOptions.CultureInvariant)]
    private static partial Regex TimerAmountRegex();
}