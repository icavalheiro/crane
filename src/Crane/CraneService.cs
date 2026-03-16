using Crane.Cli;
using Crane.Configuration.Models;

namespace Crane;

public sealed class CraneService(CraneLogger logger)
{
    public async Task RunCycleAsync(IReadOnlyList<ComposeConfiguration> services, CancellationToken cancellationToken)
    {
        logger.Log("Starting pull cycle.");

        foreach (var service in services)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var composeCli = new ComposeCli(new DirectoryInfo(service.Path), service.FileName);
            logger.Log($"[{service.Service}] Running pull in '{service.Path}'.");

            var (pullSuccess, updatedContainers, pullError) = await composeCli.Pull();
            if (!pullSuccess)
            {
                logger.Log($"[{service.Service}] Pull failed: {SanitizeMessage(pullError)}");
                continue;
            }

            logger.Log($"[{service.Service}] Pull completed. Updated images: {updatedContainers}.");

            if (updatedContainers <= 0)
                continue;

            logger.Log($"[{service.Service}] Changes detected. Running up.");
            var (upSuccess, upError) = await composeCli.Up();

            if (!upSuccess)
            {
                logger.Log($"[{service.Service}] Up failed: {SanitizeMessage(upError)}");
                continue;
            }

            logger.Log($"[{service.Service}] Up completed successfully.");
        }

        logger.Log("Pull cycle finished.");
    }

    private static string SanitizeMessage(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "No error output.";

        return value.Replace(Environment.NewLine, " | ").Trim();
    }
}