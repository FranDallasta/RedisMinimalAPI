using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Setup Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "RedisMinimalAPI:";
});

var app = builder.Build();

app.MapGet("/", () => "Redis is configured correctly!");

app.MapPost("/store", async (IDistributedCache cache) =>
{
    // Unique key to identify the data
    string key = "product1";

    // Saved value
    string value = "Product: EcoBottle, Price: $15";

    // Convert the value to bytes (Redis stores data as byte arrays)
    byte[] valueBytes = System.Text.Encoding.UTF8.GetBytes(value);

    // Configuration for cache entry
    var options = new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Expires in 5 minutes
    };

    // Store the data
    await cache.SetAsync(key, valueBytes, options);

    return Results.Ok("Data stored in Redis successfully.");
});

app.MapGet("/retrieve", async (IDistributedCache cache) =>
{
    string key = "product1";

    //Get the data from the cache
    byte[]? cachedValue = await cache.GetAsync(key);

    if (cachedValue != null)
    {
        // Convert bytes value to string
        string value = System.Text.Encoding.UTF8.GetString(cachedValue);
        return Results.Ok($"Retrieve value: {value}");
    }
    else
    {
        return Results.NotFound("The data was not found in the cache.");
    }
});

app.MapDelete("/remove", async (IDistributedCache cache) =>
{
    string key = "product1";

    // Check if the key exists
    byte[]? cachedValue = await cache.GetAsync(key);

    if (cachedValue == null)
    {
        // Key not found
        return Results.NotFound($"Key '{key}' not found in Redis.");
    }

    // Delete the key
    await cache.RemoveAsync(key);

    return Results.Ok($"Key '{key}' removed from Redis.");
});


app.MapGet("/monitor", async (IDistributedCache cache) =>
{
    string key = "product1";

    byte[]? cachedValue = await cache.GetAsync(key);

    if (cachedValue != null)
    {
        Console.WriteLine("Cache hit for product1.");
        return Results.Ok("Cache hit.");
    }
    else
    {
        Console.WriteLine("Cache miss for product1.");
        return Results.NotFound("Cache miss.");
    }
});

app.Run();
