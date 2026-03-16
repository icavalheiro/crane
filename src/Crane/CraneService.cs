using Crane.Cli;
using Crane.Configuration.Models;

namespace Crane;

public sealed class CraneService()
{
    public async Task RunServiceLoopAsync(ComposeConfiguration service, CancellationToken cancellationToken)
    {
        CraneLogger.Log($"[{service.Service}] Starting watch loop. Pull interval: {FormatInterval(service.Timer)}.");
        await RunServiceCycleAsync(service, cancellationToken);

        using var timer = new PeriodicTimer(service.Timer);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await RunServiceCycleAsync(service, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            CraneLogger.Log($"[{service.Service}] Watch loop stopped.");
        }
    }

    private async Task RunServiceCycleAsync(ComposeConfiguration service, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var composeCli = new ComposeCli(new DirectoryInfo(service.Path), service.FileName);
        CraneLogger.Log($"[{service.Service}] Running pull in '{service.Path}'.");

        var (pullSuccess, updatedContainers, pullError) = await composeCli.Pull();
        if (!pullSuccess)
        {
            CraneLogger.Log($"[{service.Service}] Pull failed: {SanitizeMessage(pullError)}");
            return;
        }

        CraneLogger.Log($"[{service.Service}] Pull completed. Updated images: {updatedContainers}.");

        if (updatedContainers <= 0)
            return;

        CraneLogger.Log($"[{service.Service}] Changes detected. Running up.");
        var (upSuccess, upError) = await composeCli.Up();

        if (!upSuccess)
        {
            CraneLogger.Log($"[{service.Service}] Up failed: {SanitizeMessage(upError)}");
            return;
        }

        CraneLogger.Log($"[{service.Service}] Up completed successfully.");
    }

    private static string SanitizeMessage(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "No error output.";

        return value.Replace(Environment.NewLine, " | ").Trim();
    }

    private static string FormatInterval(TimeSpan timer)
    {
        if (timer.TotalSeconds < 60)
            return $"{timer.TotalSeconds:0} seconds";

        if (timer.TotalMinutes < 60)
            return $"{timer.TotalMinutes:0} minutes";

        if (timer.TotalHours < 24)
            return $"{timer.TotalHours:0} hours";

        return $"{timer.TotalDays:0} days";
    }
}