using Crane;
using Crane.Configuration;

var configuration = new CraneConfig();
var services = configuration.ComposeFiles;
var craneService = new CraneService();
using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    CraneLogger.Log("Shutdown signal received. Stopping Crane...");
    cancellationTokenSource.Cancel();
};

CraneLogger.Log($"Loaded {services.Count} compose configuration entries.");
CraneLogger.Log("Starting watch loops for all services in parallel.");

var serviceTasks = services
    .Select(service => craneService.RunServiceLoopAsync(service, cancellationTokenSource.Token))
    .ToArray();

try
{
    await Task.WhenAll(serviceTasks);
}
catch (OperationCanceledException)
{
    CraneLogger.Log("Crane stopped.");
}
