namespace Crane.Configuration.Models;

public sealed record ComposeConfiguration
{
    public required string Service { get; init; }

    public required string Path { get; init; }

    public string? FileName { get; init; }
}