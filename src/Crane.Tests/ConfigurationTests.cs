using System.Collections;
using Crane.Configuration;

namespace Crane.Tests;

[NotInParallel]
public class ConfigurationTests
{
    [Test]
    public async Task Constructor_ShouldLoadComposeEntriesFromEnvironmentVariables()
    {
        using var scope = new EnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["CRANE_BLOG_PATH"] = "/var/html",
            ["CRANE_ADMIN_PATH"] = "/usr/admin/services",
            ["CRANE_ADMIN_FILE_NAME"] = "services.docker-compose.yml",
            ["CRANE_BLOG_TIMER"] = "1h",
            ["CRANE_ADMIN_TIMER"] = "5 minutes"
        });

        var configuration = new CraneConfig();

        await Assert.That(configuration.ComposeFiles.Count).IsEqualTo(2);
        await Assert.That(configuration.ComposeFiles[0].Service).IsEqualTo("ADMIN");
        await Assert.That(configuration.ComposeFiles[0].FileName).IsEqualTo("services.docker-compose.yml");
        await Assert.That(configuration.ComposeFiles[0].Timer).IsEqualTo(TimeSpan.FromMinutes(5));
        await Assert.That(configuration.ComposeFiles[1].Service).IsEqualTo("BLOG");
        await Assert.That(configuration.ComposeFiles[1].Path).IsEqualTo("/var/html");
        await Assert.That(configuration.ComposeFiles[1].Timer).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task Constructor_ShouldUseDefaultTimer_WhenServiceTimerIsNotDefined()
    {
        using var scope = new EnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["CRANE_BLOG_PATH"] = "/var/html"
        });

        var configuration = new CraneConfig();

        await Assert.That(configuration.ComposeFiles[0].Timer).IsEqualTo(TimeSpan.FromMinutes(5));
    }

    [Test]
    public async Task Constructor_ShouldThrow_WhenTimerIsInvalid()
    {
        using var scope = new EnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["CRANE_BLOG_PATH"] = "/var/html",
            ["CRANE_BLOG_TIMER"] = "tomorrow"
        });

        await Assert.That(() => new CraneConfig())
            .Throws<InvalidOperationException>()
            .WithMessage("Environment variable 'CRANE_BLOG_TIMER' has an invalid timer 'tomorrow'. Use values like '5m', '5 minutes', '1h', '30s' or '2d'.");
    }

    [Test]
    public async Task Constructor_ShouldThrow_WhenPathVariableIsEmpty()
    {
        using var scope = new EnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["CRANE_ADMIN_PATH"] = ""
        });

        await Assert.That(() => new CraneConfig())
            .Throws<InvalidOperationException>()
            .WithMessage("Environment variable 'CRANE_ADMIN_PATH' must define a path.");
    }

    [Test]
    public async Task Constructor_ShouldThrow_WhenNoServicesAreConfigured()
    {
        using var scope = new EnvironmentVariableScope(new Dictionary<string, string?>());

        await Assert.That(() => new CraneConfig())
            .Throws<InvalidOperationException>()
            .WithMessage("No compose configurations found in environment variables.");
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly Dictionary<string, string?> _originalValues = new(StringComparer.Ordinal);
        private readonly List<string> _managedKeys = [];

        public EnvironmentVariableScope(IReadOnlyDictionary<string, string?> variables)
        {
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                if (entry.Key is string key && key.StartsWith("CRANE_", StringComparison.Ordinal))
                {
                    _managedKeys.Add(key);
                    _originalValues[key] = entry.Value?.ToString();
                    Environment.SetEnvironmentVariable(key, null);
                }
            }

            foreach (var pair in variables)
            {
                if (!_managedKeys.Contains(pair.Key))
                {
                    _managedKeys.Add(pair.Key);
                    _originalValues.TryAdd(pair.Key, Environment.GetEnvironmentVariable(pair.Key));
                }

                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }

        public void Dispose()
        {
            foreach (var key in _managedKeys)
            {
                _originalValues.TryGetValue(key, out var value);
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}