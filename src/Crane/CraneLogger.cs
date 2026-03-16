namespace Crane;

public sealed class CraneLogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }
}