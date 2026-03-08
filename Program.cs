using Amazon.DynamoDBv2;
using Software_architecture_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AWS services
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddHttpClient<AwsApiService>();
builder.Services.AddScoped<AwsApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

// Add default route to serve index.html
app.MapFallbackToFile("index.html");

app.Run();
