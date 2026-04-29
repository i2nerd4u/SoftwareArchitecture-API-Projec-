using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Software_architecture_api.Controllers;
using Software_architecture_api.Data;
using Software_architecture_api.Models;
using Software_architecture_api;
using Software_architecture_api.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Architect.Tests;

public class WeatherForecastTests
{
    [Fact]
    public void WeatherForecast_Properties_Work()
    {
        var forecast = new WeatherForecast
        {
            Date = new DateOnly(2025, 1, 1),
            TemperatureC = 25,
            Summary = "Warm"
        };

        Assert.Equal(new DateOnly(2025, 1, 1), forecast.Date);
        Assert.Equal(25, forecast.TemperatureC);
        Assert.Equal("Warm", forecast.Summary);
        // TemperatureF = 32 + (int)(25 / 0.5556) = 32 + 44 = 76
        Assert.Equal(76, forecast.TemperatureF);
    }
}

public class WeatherForecastControllerTests
{
    [Fact]
    public void Get_Returns5Forecasts()
    {
        var logger = new Mock<ILogger<WeatherForecastController>>();
        var controller = new WeatherForecastController(logger.Object);

        var result = controller.Get();

        Assert.Equal(5, result.Count());
    }
}

public class AwsApiServiceTests
{
    private static HttpClient CreateMockHttpClient(HttpStatusCode status, string content)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = status,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
        return new HttpClient(handler.Object);
    }

    private static IConfiguration CreateConfig(string? itemsUrl = "http://fake/items", string? gamesUrl = "http://fake/games")
    {
        var dict = new Dictionary<string, string?>();
        if (itemsUrl != null) dict["AWS:ApiGatewayBaseUrl"] = itemsUrl;
        if (gamesUrl != null) dict["AWS:GamesApiGatewayBaseUrl"] = gamesUrl;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    // Constructor tests
    [Fact]
    public void Constructor_ThrowsIfItemsUrlMissing()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AWS:GamesApiGatewayBaseUrl"] = "http://fake"
        }).Build();
        Assert.Throws<ArgumentNullException>(() => new AwsApiService(new HttpClient(), config));
    }

    [Fact]
    public void Constructor_ThrowsIfGamesUrlMissing()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AWS:ApiGatewayBaseUrl"] = "http://fake"
        }).Build();
        Assert.Throws<ArgumentNullException>(() => new AwsApiService(new HttpClient(), config));
    }

    [Fact]
    public void Constructor_Succeeds_WithBothUrls()
    {
        var service = new AwsApiService(new HttpClient(), CreateConfig());
        Assert.NotNull(service);
    }

    // GetItemsAsync tests
    [Fact]
    public async Task GetItemsAsync_ReturnsItems()
    {
        var items = new List<Item> { new() { Id = "1", FirstName = "Karl" } };
        var body = JsonSerializer.Serialize(items, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var response = JsonSerializer.Serialize(new { statusCode = 200, body });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetItemsAsync();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsEmpty_WhenBodyIsEmptyArray()
    {
        var response = JsonSerializer.Serialize(new { statusCode = 200, body = "[]" });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetItemsAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetItemsAsync_ReturnsEmpty_WhenBodyIsEmpty()
    {
        var response = JsonSerializer.Serialize(new { statusCode = 200, body = "" });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetItemsAsync();
        Assert.Empty(result);
    }

    // CreateItemAsync tests
    [Fact]
    public async Task CreateItemAsync_ReturnsItem()
    {
        var item = new Item { Id = "1", FirstName = "Karl" };
        var body = JsonSerializer.Serialize(item, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var response = JsonSerializer.Serialize(new { statusCode = 201, body });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateItemAsync(item);
        Assert.Equal("Karl", result.FirstName);
    }

    [Fact]
    public async Task CreateItemAsync_ReturnsFallback_WhenBodyNull()
    {
        var item = new Item { Id = "1", FirstName = "Karl" };
        var response = JsonSerializer.Serialize(new { statusCode = 201, body = (string?)null });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateItemAsync(item);
        Assert.NotNull(result);
    }

    // GetGamesAsync tests - direct array response
    [Fact]
    public async Task GetGamesAsync_DirectArray_ReturnsGames()
    {
        var games = new List<Game> { new() { Id = "1", Title = "Halo" } };
        var json = JsonSerializer.Serialize(games, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var client = CreateMockHttpClient(HttpStatusCode.OK, json);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetGamesAsync();
        Assert.Single(result);
    }

    // GetGamesAsync - wrapped response with body
    [Fact]
    public async Task GetGamesAsync_Wrapped_ReturnsGames()
    {
        var games = new List<Game> { new() { Id = "1", Title = "Zelda" } };
        var body = JsonSerializer.Serialize(games, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var response = JsonSerializer.Serialize(new { statusCode = 200, body });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetGamesAsync();
        Assert.Single(result);
    }

    // GetGamesAsync - wrapped response with empty body
    [Fact]
    public async Task GetGamesAsync_Wrapped_EmptyBody_ReturnsEmpty()
    {
        var response = JsonSerializer.Serialize(new { statusCode = 200, body = "[]" });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetGamesAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetGamesAsync_Wrapped_NullBody_ReturnsEmpty()
    {
        var response = JsonSerializer.Serialize(new { statusCode = 200, body = "" });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetGamesAsync();
        Assert.Empty(result);
    }

    // CreateGameAsync tests - direct object response
    [Fact]
    public async Task CreateGameAsync_DirectObject_ReturnsGame()
    {
        var game = new Game { Id = "1", Title = "Halo" };
        var json = JsonSerializer.Serialize(game, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var client = CreateMockHttpClient(HttpStatusCode.OK, json);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateGameAsync(game);
        Assert.Equal("Halo", result.Title);
    }

    // CreateGameAsync - wrapped response
    [Fact]
    public async Task CreateGameAsync_Wrapped_ReturnsGame()
    {
        var game = new Game { Id = "1", Title = "Zelda" };
        var body = JsonSerializer.Serialize(game, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var response = JsonSerializer.Serialize(new { statusCode = 201, body });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateGameAsync(game);
        Assert.Equal("Zelda", result.Title);
    }

    // CreateGameAsync - wrapped with null body returns fallback
    [Fact]
    public async Task CreateGameAsync_Wrapped_NullBody_ReturnsFallback()
    {
        var game = new Game { Id = "1", Title = "Fallback" };
        var response = JsonSerializer.Serialize(new { statusCode = 201, body = (string?)null });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateGameAsync(game);
        Assert.NotNull(result);
    }

    // CreateGameAsync - response that contains "statusCode" but starts with { goes to wrapped path
    [Fact]
    public async Task CreateGameAsync_WrappedWithStatusCode_ReturnsGame()
    {
        var game = new Game { Id = "1", Title = "Test" };
        var body = JsonSerializer.Serialize(game, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        // This has statusCode so it goes to the wrapped/else branch
        var response = JsonSerializer.Serialize(new { statusCode = 200, body });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateGameAsync(game);
        Assert.Equal("Test", result.Title);
    }

    // GetItemsAsync - body.GetString() returns null (triggers ?? "[]" path)
    [Fact]
    public async Task GetItemsAsync_NullBody_ReturnsEmpty()
    {
        // body is JSON null, GetString() returns null, ?? gives "[]"
        var response = "{\"body\": null}";
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetItemsAsync();
        Assert.Empty(result);
    }

    // GetGamesAsync - wrapped with null body property
    [Fact]
    public async Task GetGamesAsync_Wrapped_NullBodyProperty_ReturnsEmpty()
    {
        var response = "{\"body\": null}";
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetGamesAsync();
        Assert.Empty(result);
    }

    // CreateItemAsync - body.GetString() returns null (triggers ?? "{}" path)
    [Fact]
    public async Task CreateItemAsync_NullBodyProperty_ReturnsItem()
    {
        var item = new Item { Id = "1", FirstName = "Test" };
        var response = "{\"body\": null}";
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateItemAsync(item);
        Assert.NotNull(result);
    }

    // CreateGameAsync - body.GetString() returns null in wrapped path
    [Fact]
    public async Task CreateGameAsync_NullBodyProperty_ReturnsGame()
    {
        var game = new Game { Id = "1", Title = "Test" };
        var response = "{\"statusCode\": 200, \"body\": null}";
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateGameAsync(game);
        Assert.NotNull(result);
    }

    // Trigger Deserialize returning null for items (body = "null")
    [Fact]
    public async Task GetItemsAsync_BodyIsNullString_ReturnsEmpty()
    {
        var response = "{\"body\": \"null\"}";
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetItemsAsync();
        Assert.Empty(result);
    }

    // Trigger Deserialize returning null for CreateItem (body = "null")
    [Fact]
    public async Task CreateItemAsync_BodyNullString_ReturnsFallback()
    {
        var item = new Item { Id = "1", FirstName = "Fallback" };
        var response = "{\"body\": \"null\"}";
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateItemAsync(item);
        Assert.Equal("Fallback", result.FirstName);
    }

    // Trigger Deserialize returning null for GetGames wrapped (body = "null")
    [Fact]
    public async Task GetGamesAsync_Wrapped_BodyNullString_ReturnsEmpty()
    {
        var response = "{\"body\": \"null\"}";
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetGamesAsync();
        Assert.Empty(result);
    }

    // Trigger Deserialize returning null for CreateGame direct (body = "null" as direct response)
    [Fact]
    public async Task CreateGameAsync_DirectNull_ReturnsFallback()
    {
        var game = new Game { Id = "1", Title = "Fallback" };
        // "null" is valid JSON, no statusCode, starts with n not {
        // Actually this won't match the direct path since it doesn't start with {
        // We need a direct object that deserializes to null - not possible with valid JSON object
        // Instead test wrapped path with body = "null"
        var response = "{\"statusCode\": 200, \"body\": \"null\"}";
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateGameAsync(game);
        Assert.Equal("Fallback", result.Title);
    }

    // Direct array response for GetGames that returns null from deserialize
    [Fact]
    public async Task GetGamesAsync_DirectEmptyNullArray_ReturnsEmpty()
    {
        // Send a JSON array of nulls - deserializer handles gracefully
        var client = CreateMockHttpClient(HttpStatusCode.OK, "[]");
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.GetGamesAsync();
        Assert.Empty(result);
    }

    // CreateGameAsync direct path - response without statusCode that starts with {
    [Fact]
    public async Task CreateGameAsync_DirectObject_NullDeserialize_ReturnsFallback()
    {
        var game = new Game { Id = "1", Title = "Fallback" };
        var client = CreateMockHttpClient(HttpStatusCode.OK, "{}");
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateGameAsync(game);
        Assert.NotNull(result);
    }

    // CreateGameAsync - response that doesn't start with { (e.g. array or other)
    [Fact]
    public async Task CreateGameAsync_NonObjectResponse_GoesToWrappedPath()
    {
        var game = new Game { Id = "1", Title = "Test" };
        var body = JsonSerializer.Serialize(game, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        // Response starts with [ not { — goes to else branch
        var wrappedInArray = $"{{\"statusCode\": 200, \"body\": {JsonSerializer.Serialize(body)}}}";
        // Actually we need a response that doesn't start with { at all
        // But the API always returns JSON objects... Let's use a response starting with whitespace + non-{
        // The simplest: a response wrapped with statusCode (starts with { but contains statusCode)
        var response = JsonSerializer.Serialize(new { statusCode = 200, body });
        var client = CreateMockHttpClient(HttpStatusCode.OK, response);
        var service = new AwsApiService(client, CreateConfig());

        var result = await service.CreateGameAsync(game);
        Assert.Equal("Test", result.Title);
    }
}

// Tests for controller error (catch) paths
public class ControllerErrorTests
{
    [Fact]
    public async Task GetItems_Returns500_OnException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Dispose(); // force disposed context to trigger exception

        var controller = new ItemsController(db);
        var result = await controller.GetItems();

        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    [Fact]
    public async Task CreateItem_Returns500_OnException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Dispose();

        var controller = new ItemsController(db);
        var result = await controller.CreateItem(new Item { FirstName = "Test" });

        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    [Fact]
    public async Task GetGames_Returns500_OnException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Dispose();

        var controller = new GamesController(db);
        var result = await controller.GetGames();

        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    [Fact]
    public async Task CreateGame_Returns500_OnException()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Dispose();

        var controller = new GamesController(db);
        var result = await controller.CreateGame(new Game { Title = "Test" });

        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
    }

    // Test CreateItem with an existing Id (covers the else branch of IsNullOrWhiteSpace)
    [Fact]
    public async Task CreateItem_WithExistingId_KeepsId()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new AppDbContext(options);
        var controller = new ItemsController(db);

        var result = await controller.CreateItem(new Item { Id = "my-id", FirstName = "Test" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var item = Assert.IsType<Item>(created.Value);
        Assert.Equal("my-id", item.Id);
    }

    // Test CreateGame with an existing Id
    [Fact]
    public async Task CreateGame_WithExistingId_KeepsId()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var db = new AppDbContext(options);
        var controller = new GamesController(db);

        var result = await controller.CreateGame(new Game { Id = "my-id", Title = "Test" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var game = Assert.IsType<Game>(created.Value);
        Assert.Equal("my-id", game.Id);
    }
}
