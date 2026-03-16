using Crane.Cli.Models;

namespace Crane.Cli;

public class ComposeCli(DirectoryInfo directory, string? fileName)
{
    public static async Task<CliResult> SystemPrune()
    {
        return await DockerCli.ExecuteCommand("system prune -f", Directory.GetCurrentDirectory());
    }

    private async Task<CliResult> ExecuteCommand(string arguments)
    {
        return await DockerCli.ExecuteCommand(arguments, directory.FullName);
    }

    private string GetBaseComposeCommand()
    {
        var cmd = "compose";
        if (fileName is not null)
            cmd += $" -f {fileName}";

        return cmd;
    }

    public async Task<(bool Success, string Error)> Up()
    {
        var result = await ExecuteCommand($"{GetBaseComposeCommand()} up -d");
        return (result.IsSuccess, result.StdErr);
    }

    public async Task<(bool Success, string Error)> Stop()
    {
        var result = await ExecuteCommand($"{GetBaseComposeCommand()} stop");
        return (result.IsSuccess, result.StdErr);
    }

    public async Task<(bool Success, string Error)> Down()
    {
        var result = await ExecuteCommand($"{GetBaseComposeCommand()} down");
        return (result.IsSuccess, result.StdErr);
    }

    public async Task<(bool Success, int UpdatedContainers, string Error)> Pull()
    {
        var result = await ExecuteCommand($"{GetBaseComposeCommand()} pull");
        var updatedContainers = (result.StdOut + result.StdErr)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Count(line => line.Contains("Pulled", StringComparison.OrdinalIgnoreCase));

        return (result.IsSuccess, updatedContainers, result.StdErr);
    }

    public async Task<(bool Success, string Output, string Error)> Ps()
    {
        var result = await ExecuteCommand($"{GetBaseComposeCommand()} ps");
        return (result.IsSuccess, result.StdOut, result.StdErr);
    }
}