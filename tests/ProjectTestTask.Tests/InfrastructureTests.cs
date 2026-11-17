
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ProjectTestTask.Tests;

public class InfrastructureTests
{
    [Fact]
    [Trait("Criterion", "1")]
    public async Task MongoDB_ShouldBeHealthy()
    {
        await WaitUntilHealthy(
            async () =>
            {
                var containerId = await CommandRunner.Run(
                    "docker", "compose", "ps", "-q", "mongodb");

                if (string.IsNullOrWhiteSpace(containerId))
                    return (false, "Container MongoDB não encontrado");

                var status = await CommandRunner.Run(
                    "docker", "inspect",
                    "--format", "{{.State.Health.Status}}",
                    containerId);

                return (status == "healthy", $"MongoDB: {status}");
            },
            "MongoDB",
            TimeSpan.FromSeconds(90));

        var ping = await CommandRunner.Run(
            "docker", "compose", "exec", "-T", "mongodb",
            "mongosh", "--quiet", "--eval",
            "db.adminCommand({ping:1}).ok");

        Assert.Equal("1", ping.Trim());
    }

    [Fact]
    [Trait("Criterion", "1")]
    public async Task ServiceBus_ShouldBeHealthy()
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        await WaitUntilHealthy(
            async () =>
            {
                using var response = await client.GetAsync(
                    "http://localhost:5300/health");

                var body = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK)
                    return (false, $"HTTP {(int)response.StatusCode}: {body}");

                using var json = JsonDocument.Parse(body);
                var status = json.RootElement
                    .GetProperty("status").GetString();

                return (status == "healthy", $"Service Bus: {status}");
            },
            "Service Bus",
            TimeSpan.FromMinutes(3));
    }

    private static async Task WaitUntilHealthy(
        Func<Task<(bool Healthy, string Detail)>> check,
        string service,
        TimeSpan timeout)
    {
        var watch = Stopwatch.StartNew();
        var lastObservation = "Sem resposta";

        while (watch.Elapsed < timeout)
        {
            try
            {
                var result = await check();

                if (result.Healthy)
                    return;

                lastObservation = result.Detail;
            }
            catch (Exception ex)
            {
                lastObservation = ex.Message;
            }

            await Task.Delay(TimeSpan.FromSeconds(3));
        }

        Assert.Fail(
            $"{service} não ficou saudável em " +
            $"{timeout.TotalSeconds}s. " +
            $"Última observação: {lastObservation}");
    }
}
