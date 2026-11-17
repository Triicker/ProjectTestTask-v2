using System.Diagnostics;

namespace ProjectTestTask.Tests;

public class EnvironmentTests
{
    [Fact]
    [Trait("Criterion", "0")]
    public async Task Dotnet10_ShouldBeAvailable()
    {
        var version = await CommandRunner.Run("dotnet", "--version");

        Assert.StartsWith("10.", version);
    }

    [Fact]
    [Trait("Criterion", "0")]
    public async Task DockerDaemon_ShouldRespond()
    {
        await CommandRunner.Run("docker", "ps");
    }
}

internal static class CommandRunner
{
    public static async Task<string> Run(
        string executable,
        params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(executable)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = startInfo };

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await process.WaitForExitAsync(timeout.Token);

        var output = await outputTask;
        var error = await errorTask;

        Assert.True(
            process.ExitCode == 0,
            $"{executable} failed: {error}");

        return output.Trim();
    }
}