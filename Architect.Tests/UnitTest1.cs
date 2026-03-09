using Xunit;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Software_architecture_api.Controllers;
using Software_architecture_api.Models;
using Software_architecture_api.Services;

namespace Architect.Tests;

public class UnitTest1
{
    // Test 1: Check that items are returned correctly
    [Fact]
    public async Task GetItems_ReturnsItems()
    {
        string json = """
        {
          "body": "[{\"id\":\"1\",\"firstName\":\"John\",\"lastName\":\"Halo\",\"funFact\":\"Great game\"}]"
        }
        """;

        var controller = CreateController(HttpStatusCode.OK, json);

        var result = await controller.GetItems();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsType<List<Item>>(ok.Value);

        Assert.Equal("John", items[0].FirstName);
    }

    // Test 2: Check that an empty list works
    [Fact]
    public async Task GetItems_ReturnsEmptyList()
    {
        string json = """{ "body": "[]" }""";

        var controller = CreateController(HttpStatusCode.OK, json);

        var result = await controller.GetItems();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsType<List<Item>>(ok.Value);

        Assert.Empty(items);
    }

    // Test 3: Check that creating an item works
    [Fact]
    public async Task CreateItem_ReturnsItem()
    {
        string json = """
        {
          "body": "{\"id\":\"123\",\"firstName\":\"Alice\",\"lastName\":\"Minecraft\",\"funFact\":\"Great game\"}"
        }
        """;

        var controller = CreateController(HttpStatusCode.OK, json);

        var item = new Item
        {
            FirstName = "Alice",
            LastName = "Minecraft",
            FunFact = "Great game"
        };

        var result = await controller.CreateItem(item);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedItem = Assert.IsType<Item>(created.Value);

        Assert.Equal("Alice", returnedItem.FirstName);
    }

    private ItemsController CreateController(HttpStatusCode code, string response)
    {
        var handler = new FakeHandler(code, response);
        var httpClient = new HttpClient(handler);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AWS:ApiGatewayBaseUrl", "https://fake-url" }
            })
            .Build();

        var service = new AwsApiService(httpClient, config);

        return new ItemsController(service);
    }

    class FakeHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _code;
        private readonly string _response;

        public FakeHandler(HttpStatusCode code, string response)
        {
            _code = code;
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var message = new HttpResponseMessage(_code);
            message.Content = new StringContent(_response, Encoding.UTF8, "application/json");

            return Task.FromResult(message);
        }
    }
}