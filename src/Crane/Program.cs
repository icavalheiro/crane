using Crane;
using Crane.Configuration;

var configuration = new CraneConfig();
var services = configuration.ComposeFiles;
var interval = TimeSpan.FromMinutes(5);
var logger = new CraneLogger();
var craneService = new CraneService(logger);
using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    logger.Log("Shutdown signal received. Stopping Crane...");
    cancellationTokenSource.Cancel();
};

logger.Log($"Loaded {services.Count} compose configuration entries.");
logger.Log($"Starting watch loop. Pull interval: {interval.TotalMinutes:0} minutes.");

//run first cycle right away
await craneService.RunCycleAsync(services, cancellationTokenSource.Token);

//start time for next cycles
using var timer = new PeriodicTimer(interval);
try
{
    while (await timer.WaitForNextTickAsync(cancellationTokenSource.Token))
    {
        await craneService.RunCycleAsync(services, cancellationTokenSource.Token);
    }
}
catch (OperationCanceledException)
{
    logger.Log("Crane stopped.");
}
