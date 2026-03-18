using System.Text;
using System.Text.Json;
using Software_architecture_api.Models;

namespace Software_architecture_api.Services;

public class AwsApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _gamesBaseUrl;

    public AwsApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AWS:ApiGatewayBaseUrl"] ?? throw new ArgumentNullException("AWS:ApiGatewayBaseUrl");
        _gamesBaseUrl = configuration["AWS:GamesApiGatewayBaseUrl"] ?? throw new ArgumentNullException("AWS:GamesApiGatewayBaseUrl");
    }

    public async Task<List<Item>> GetItemsAsync()
    {
        var response = await _httpClient.GetAsync(_baseUrl);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        
        // Debug: Log the actual JSON response
        Console.WriteLine($"Raw JSON response: {json}");
        
        // Parse the API Gateway response
        var apiResponse = JsonSerializer.Deserialize<JsonElement>(json);
        var bodyJson = apiResponse.GetProperty("body").GetString() ?? "[]";
        
        Console.WriteLine($"Body JSON: {bodyJson}");
        
        // Handle case where response might be empty array
        if (string.IsNullOrEmpty(bodyJson) || bodyJson == "[]")
        {
            return new List<Item>();
        }
        
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        return JsonSerializer.Deserialize<List<Item>>(bodyJson, options) ?? new List<Item>();
    }

    public async Task<Item> CreateItemAsync(Item item)
    {
        var json = JsonSerializer.Serialize(item, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        Console.WriteLine($"Sending to Lambda: {json}");
        
        var response = await _httpClient.PostAsync(_baseUrl, content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Lambda response: {responseJson}");
        
        // Parse the API Gateway response
        var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);
        var bodyJson = apiResponse.GetProperty("body").GetString() ?? "{}";
        
        Console.WriteLine($"Lambda body: {bodyJson}");
        
        var options = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        return JsonSerializer.Deserialize<Item>(bodyJson, options) ?? item;
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        var response = await _httpClient.GetAsync(_gamesBaseUrl);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Raw Games JSON: {json}");
        
        // Check if response is wrapped in API Gateway format or direct array
        if (json.TrimStart().StartsWith("["))
        {
            // Direct array response
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return JsonSerializer.Deserialize<List<Game>>(json, options) ?? new List<Game>();
        }
        else
        {
            // API Gateway wrapped response
            var apiResponse = JsonSerializer.Deserialize<JsonElement>(json);
            var bodyJson = apiResponse.GetProperty("body").GetString() ?? "[]";
            
            if (string.IsNullOrEmpty(bodyJson) || bodyJson == "[]")
                return new List<Game>();
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return JsonSerializer.Deserialize<List<Game>>(bodyJson, options) ?? new List<Game>();
        }
    }

    public async Task<Game> CreateGameAsync(Game game)
    {
        var json = JsonSerializer.Serialize(game, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        Console.WriteLine($"Sending Game: {json}");
        
        var response = await _httpClient.PostAsync(_gamesBaseUrl, content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Game Response: {responseJson}");
        
        // Check if response is wrapped or direct
        if (responseJson.TrimStart().StartsWith("{") && !responseJson.Contains("statusCode"))
        {
            // Direct object response
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return JsonSerializer.Deserialize<Game>(responseJson, options) ?? game;
        }
        else
        {
            // API Gateway wrapped response
            var apiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);
            var bodyJson = apiResponse.GetProperty("body").GetString() ?? "{}";
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return JsonSerializer.Deserialize<Game>(bodyJson, options) ?? game;
        }
    }
}