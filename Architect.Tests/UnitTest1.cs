using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_architecture_api.Controllers;
using Software_architecture_api.Data;
using Software_architecture_api.Models;

namespace Architect.Tests;

public class UnitTest1
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // --- Items Tests ---

    // Test 1: GET /api/items returns all items
    [Fact]
    public async Task GetItems_ReturnsAllItems()
    {
        using var db = CreateDb();
        db.Items.Add(new Item { Id = "1", FirstName = "Karl", LastName = "Halo", FunFact = "Great game", Platform = "Xbox" });
        db.Items.Add(new Item { Id = "2", FirstName = "Ethan", LastName = "Minecraft", FunFact = "Fun game", Platform = "PC" });
        await db.SaveChangesAsync();

        var controller = new ItemsController(db);
        var result = await controller.GetItems();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsType<List<Item>>(ok.Value);
        Assert.Equal(2, items.Count);
    }

    // Test 2: GET /api/items returns empty list when no items exist
    [Fact]
    public async Task GetItems_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var controller = new ItemsController(db);

        var result = await controller.GetItems();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsType<List<Item>>(ok.Value);
        Assert.Empty(items);
    }

    // Test 3: POST /api/items creates a new review
    [Fact]
    public async Task CreateItem_AddsReviewToDatabase()
    {
        using var db = CreateDb();
        var controller = new ItemsController(db);

        var newItem = new Item { FirstName = "Karl", LastName = "Zelda", FunFact = "Amazing", Platform = "Switch" };
        var result = await controller.CreateItem(newItem);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<Item>(created.Value);
        Assert.Equal("Karl", returned.FirstName);
        Assert.Equal(1, db.Items.Count());
    }

    // Test 4: POST /api/items auto-assigns an Id if none provided
    [Fact]
    public async Task CreateItem_AssignsIdIfMissing()
    {
        using var db = CreateDb();
        var controller = new ItemsController(db);

        var result = await controller.CreateItem(new Item { FirstName = "Test", LastName = "Game" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<Item>(created.Value);
        Assert.False(string.IsNullOrWhiteSpace(returned.Id));
    }

    // Test 5: GET /api/items returns items ordered by newest first
    [Fact]
    public async Task GetItems_ReturnsMostRecentFirst()
    {
        using var db = CreateDb();
        db.Items.Add(new Item { Id = "1", FirstName = "Old", CreatedAt = DateTime.UtcNow.AddDays(-2) });
        db.Items.Add(new Item { Id = "2", FirstName = "New", CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var controller = new ItemsController(db);
        var result = await controller.GetItems();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsType<List<Item>>(ok.Value);
        Assert.Equal("New", items[0].FirstName);
    }

    // --- Games Tests ---

    // Test 6: GET /api/games returns all games
    [Fact]
    public async Task GetGames_ReturnsAllGames()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Id = "1", Title = "Halo", Genre = "FPS", Developer = "Bungie", Platform = "Xbox", ReleaseYear = "2001" });
        db.Games.Add(new Game { Id = "2", Title = "Zelda", Genre = "Adventure", Developer = "Nintendo", Platform = "Switch", ReleaseYear = "2017" });
        await db.SaveChangesAsync();

        var controller = new GamesController(db);
        var result = await controller.GetGames();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var games = Assert.IsType<List<Game>>(ok.Value);
        Assert.Equal(2, games.Count);
    }

    // Test 7: GET /api/games returns empty list when no games exist
    [Fact]
    public async Task GetGames_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var controller = new GamesController(db);

        var result = await controller.GetGames();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var games = Assert.IsType<List<Game>>(ok.Value);
        Assert.Empty(games);
    }

    // Test 8: POST /api/games creates a new game
    [Fact]
    public async Task CreateGame_AddsGameToDatabase()
    {
        using var db = CreateDb();
        var controller = new GamesController(db);

        var newGame = new Game { Title = "Minecraft", Genre = "Sandbox", Developer = "Mojang", Platform = "PC", ReleaseYear = "2011" };
        var result = await controller.CreateGame(newGame);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<Game>(created.Value);
        Assert.Equal("Minecraft", returned.Title);
        Assert.Equal(1, db.Games.Count());
    }

    // Test 9: POST /api/games auto-assigns an Id if none provided
    [Fact]
    public async Task CreateGame_AssignsIdIfMissing()
    {
        using var db = CreateDb();
        var controller = new GamesController(db);

        var result = await controller.CreateGame(new Game { Title = "TestGame" });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returned = Assert.IsType<Game>(created.Value);
        Assert.False(string.IsNullOrWhiteSpace(returned.Id));
    }

    // Test 10: GET /api/games returns games ordered alphabetically by title
    [Fact]
    public async Task GetGames_ReturnsGamesAlphabetically()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Id = "1", Title = "Zelda" });
        db.Games.Add(new Game { Id = "2", Title = "Halo" });
        await db.SaveChangesAsync();

        var controller = new GamesController(db);
        var result = await controller.GetGames();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var games = Assert.IsType<List<Game>>(ok.Value);
        Assert.Equal("Halo", games[0].Title);
        Assert.Equal("Zelda", games[1].Title);
    }
}
