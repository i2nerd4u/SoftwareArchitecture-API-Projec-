using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_architecture_api.Data;
using Software_architecture_api.Models;

namespace Software_architecture_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly AppDbContext _context;

    public GamesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Game>>> GetGames()
    {
        try
        {
            var games = await _context.Games
                .OrderBy(x => x.Title)
                .ToListAsync();

            return Ok(games);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving games: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Game>> CreateGame([FromBody] Game game)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(game.Id))
                game.Id = Guid.NewGuid().ToString();

            game.CreatedAt = DateTime.UtcNow;

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGames), new { id = game.Id }, game);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating game: {ex.Message}");
        }
    }
}