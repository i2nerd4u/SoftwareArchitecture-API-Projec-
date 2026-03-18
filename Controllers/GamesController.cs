using Microsoft.AspNetCore.Mvc;
using Software_architecture_api.Models;
using Software_architecture_api.Services;

namespace Software_architecture_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly AwsApiService _awsApiService;

    public GamesController(AwsApiService awsApiService)
    {
        _awsApiService = awsApiService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Game>>> GetGames()
    {
        try
        {
            var games = await _awsApiService.GetGamesAsync();
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
            if (string.IsNullOrEmpty(game.Id))
                game.Id = Guid.NewGuid().ToString();

            var createdGame = await _awsApiService.CreateGameAsync(game);
            return CreatedAtAction(nameof(GetGames), new { id = createdGame.Id }, createdGame);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating game: {ex.Message}");
        }
    }
}
