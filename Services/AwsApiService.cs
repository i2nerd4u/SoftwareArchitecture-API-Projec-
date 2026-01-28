using System.Text;
using System.Text.Json;
using Software_architecture_api.Models;

namespace Software_architecture_api.Services;

public class AwsApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AwsApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AWS:ApiGatewayBaseUrl"] ?? throw new ArgumentNullException("AWS:ApiGatewayBaseUrl");
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
}