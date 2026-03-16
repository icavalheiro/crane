namespace Crane;

public static class CraneLogger
{
    public static void Log(string message)
    {
        Console.WriteLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }
}