using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Software_architecture_api.Data;
using Software_architecture_api.Models;

namespace Software_architecture_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItemsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Item>>> GetItems()
    {
        try
        {
            var items = await _context.Items
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

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
            if (string.IsNullOrWhiteSpace(item.Id))
                item.Id = Guid.NewGuid().ToString();

            item.CreatedAt = DateTime.UtcNow;

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItems), new { id = item.Id }, item);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating item: {ex.Message}");
        }
    }
}