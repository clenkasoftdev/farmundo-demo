using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Farmundo.Demo.Tests;

public class ApiSmokeTests
{
    [Fact]
    public async Task Ping_ReturnsPong()
    {
        await using var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();
        var res = await client.GetAsync("/api/ping");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
