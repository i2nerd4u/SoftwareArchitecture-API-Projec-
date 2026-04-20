using Xunit;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Software_architecture_api.Data;
using Software_architecture_api.Models;

namespace Architect.Tests;

public class E2EFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("E2ETestDb"));
        });
    }
}

public class EndToEndTests : IClassFixture<E2EFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly E2EFactory _factory;

    public EndToEndTests(E2EFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Items.RemoveRange(db.Items);
        db.Games.RemoveRange(db.Games);
        await db.SaveChangesAsync();
    }

    // E2E Test 1: GET /api/items returns 200 OK with empty list initially
    [Fact]
    public async Task GET_Items_Returns200_WithEmptyList()
    {
        var response = await _client.GetAsync("/api/items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<Item>>();
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    // E2E Test 2: GET /api/games returns 200 OK with empty list initially
    [Fact]
    public async Task GET_Games_Returns200_WithEmptyList()
    {
        var response = await _client.GetAsync("/api/games");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var games = await response.Content.ReadFromJsonAsync<List<Game>>();
        Assert.NotNull(games);
        Assert.Empty(games);
    }

    // E2E Test 3: POST /api/items then GET returns the created review
    [Fact]
    public async Task POST_Item_ThenGET_ReturnsCreatedReview()
    {
        var newItem = new Item
        {
            FirstName = "Karl",
            LastName = "Halo Infinite",
            FunFact = "Best Halo in years",
            Platform = "Xbox"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/items", newItem);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var getResponse = await _client.GetAsync("/api/items");
        var items = await getResponse.Content.ReadFromJsonAsync<List<Item>>();

        Assert.NotNull(items);
        Assert.Single(items);
        Assert.Equal("Karl", items[0].FirstName);
        Assert.Equal("Halo Infinite", items[0].LastName);
        Assert.Equal("Xbox", items[0].Platform);
    }

    // E2E Test 4: POST /api/games then GET returns the created game
    [Fact]
    public async Task POST_Game_ThenGET_ReturnsCreatedGame()
    {
        var newGame = new Game
        {
            Title = "Minecraft",
            Genre = "Sandbox",
            Developer = "Mojang",
            Platform = "PC",
            ReleaseYear = "2011"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/games", newGame);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var getResponse = await _client.GetAsync("/api/games");
        var games = await getResponse.Content.ReadFromJsonAsync<List<Game>>();

        Assert.NotNull(games);
        Assert.Single(games);
        Assert.Equal("Minecraft", games[0].Title);
        Assert.Equal("Mojang", games[0].Developer);
    }

    // E2E Test 5: POST /api/items returns the created item in the response body
    [Fact]
    public async Task POST_Item_ReturnsCreatedItemInBody()
    {
        var newItem = new Item
        {
            FirstName = "Ethan",
            LastName = "Zelda",
            FunFact = "Classic adventure",
            Platform = "Switch"
        };

        var response = await _client.PostAsJsonAsync("/api/items", newItem);
        var created = await response.Content.ReadFromJsonAsync<Item>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("Ethan", created.FirstName);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));
    }

    // E2E Test 6: POST /api/games returns the created game in the response body
    [Fact]
    public async Task POST_Game_ReturnsCreatedGameInBody()
    {
        var newGame = new Game
        {
            Title = "God of War",
            Genre = "Action",
            Developer = "Santa Monica",
            Platform = "PS5",
            ReleaseYear = "2022"
        };

        var response = await _client.PostAsJsonAsync("/api/games", newGame);
        var created = await response.Content.ReadFromJsonAsync<Game>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("God of War", created.Title);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));
    }

    // E2E Test 7: Multiple reviews are returned in newest-first order
    [Fact]
    public async Task GET_Items_ReturnsMultipleReviews_NewestFirst()
    {
        await _client.PostAsJsonAsync("/api/items", new Item { FirstName = "First", LastName = "OldGame", Platform = "PC" });
        await Task.Delay(10);
        await _client.PostAsJsonAsync("/api/items", new Item { FirstName = "Second", LastName = "NewGame", Platform = "PS5" });

        var response = await _client.GetAsync("/api/items");
        var items = await response.Content.ReadFromJsonAsync<List<Item>>();

        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
        Assert.Equal("Second", items[0].FirstName);
    }

    // E2E Test 8: Multiple games are returned in alphabetical order
    [Fact]
    public async Task GET_Games_ReturnsMultipleGames_Alphabetically()
    {
        await _client.PostAsJsonAsync("/api/games", new Game { Title = "Zelda" });
        await _client.PostAsJsonAsync("/api/games", new Game { Title = "Halo" });
        await _client.PostAsJsonAsync("/api/games", new Game { Title = "Minecraft" });

        var response = await _client.GetAsync("/api/games");
        var games = await response.Content.ReadFromJsonAsync<List<Game>>();

        Assert.NotNull(games);
        Assert.Equal("Halo", games[0].Title);
        Assert.Equal("Minecraft", games[1].Title);
        Assert.Equal("Zelda", games[2].Title);
    }
}
