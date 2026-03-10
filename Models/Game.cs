namespace Software_architecture_api.Models;

public class Game
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Developer { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string ReleaseYear { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
