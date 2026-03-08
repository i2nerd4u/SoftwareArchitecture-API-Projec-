using Microsoft.AspNetCore.Mvc;
using Software_architecture_api.Models;
using Software_architecture_api.Services;

namespace Software_architecture_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AwsApiService _awsApiService;

    public ItemsController(AwsApiService awsApiService)
    {
        _awsApiService = awsApiService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Item>>> GetItems()
    {
        try
        {
            var items = await _awsApiService.GetItemsAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving items: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Item>> CreateItem([FromBody] Item item)
    {
        try
        {
            if (string.IsNullOrEmpty(item.Id))
                item.Id = Guid.NewGuid().ToString();

            var createdItem = await _awsApiService.CreateItemAsync(item);
            return CreatedAtAction(nameof(GetItems), new { id = createdItem.Id }, createdItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating item: {ex.Message}");
        }
    }
}