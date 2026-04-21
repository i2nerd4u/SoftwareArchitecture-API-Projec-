using Amazon.DynamoDBv2;
using Microsoft.EntityFrameworkCore;
using Software_architecture_api.Services;
using Software_architecture_api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Keep AWS (for now)
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddHttpClient<AwsApiService>();
builder.Services.AddScoped<AwsApiService>();

var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection(); disabled for testing
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

// Add default route to serve index.html
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }