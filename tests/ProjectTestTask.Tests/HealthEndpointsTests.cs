using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ProjectTestTask.Tests;

public class HealthEndpointsTests
{
    [Fact]
    [Trait("Criterion", "2")]
    public async Task Inventory_Health_Returns200()
    {
        await AssertHealthy<Inventory.Api.AssemblyMarker>();
    }

    [Fact]
    [Trait("Criterion", "2")]
    public async Task Trades_Health_Returns200()
    {
        await AssertHealthy<Trades.Api.AssemblyMarker>();
    }

    private static async Task AssertHealthy<T>()
        where T : class
    {
        using var factory = new WebApplicationFactory<T>();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("Healthy", body);
    }
}