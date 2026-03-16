using Crane.Cli;

namespace Crane.Tests;

[NotInParallel]
public class ComposeCliTests
{
    private static readonly string RepoRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string ComposeFile = Path.Combine(RepoRoot, "tests.docker-compose.yml");

    private static ComposeCli CreateCli() =>
        new(new DirectoryInfo(RepoRoot), ComposeFile);

    [Test]
    public async Task Pull_ShouldSucceed_AndReturnUpdatedCount()
    {
        var cli = CreateCli();

        var (success, updatedContainers, error) = await cli.Pull();

        await Assert.That(success).IsTrue();
        await Assert.That(updatedContainers).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task Up_ShouldStartContainers()
    {
        var cli = CreateCli();

        var (success, error) = await cli.Up();

        await Assert.That(success).IsTrue();

        // Cleanup
        await cli.Down();
    }

    [Test]
    public async Task Ps_ShouldReturnOutput_WhenContainersAreRunning()
    {
        var cli = CreateCli();
        await cli.Up();

        var (success, output, error) = await cli.Ps();

        await Assert.That(success).IsTrue();
        await Assert.That(output).IsNotEmpty();

        // Cleanup
        await cli.Down();
    }

    [Test]
    public async Task Stop_ShouldSucceed_WhenContainersAreRunning()
    {
        var cli = CreateCli();
        await cli.Up();

        var (success, error) = await cli.Stop();

        await Assert.That(success).IsTrue();

        // Cleanup
        await cli.Down();
    }

    [Test]
    public async Task Down_ShouldSucceed_WhenContainersAreRunning()
    {
        var cli = CreateCli();
        await cli.Up();

        var (success, error) = await cli.Down();

        await Assert.That(success).IsTrue();
    }
}
