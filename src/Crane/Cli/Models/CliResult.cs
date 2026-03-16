namespace Crane.Cli.Models;

public record CliResult(string StdOut, string StdErr, int ExitCode)
{
    public bool IsSuccess => ExitCode == 0;
};