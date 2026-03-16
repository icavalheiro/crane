using System.Diagnostics;
using System.Text;
using Crane.Cli.Models;

namespace Crane.Cli;

public static class DockerCli
{
    public static async Task<CliResult> ExecuteCommand(string arguments, string dir)
    {
        var stdOutBuffer = new StringBuilder();
        var stdErrBuffer = new StringBuilder();

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            WorkingDirectory = dir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = processStartInfo };

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                stdOutBuffer.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                stdErrBuffer.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return new CliResult(stdOutBuffer.ToString(), stdErrBuffer.ToString(), process.ExitCode);
    }
}