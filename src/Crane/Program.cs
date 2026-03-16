using Crane.Configuration;

var configuration = new CraneConfig();

Console.WriteLine($"Loaded {configuration.ComposeFiles.Count} compose configuration entries.");
