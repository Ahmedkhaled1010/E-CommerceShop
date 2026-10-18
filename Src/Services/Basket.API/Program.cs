
using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var assembly = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("BasketDb")!);
    options.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
// Add caching decorator for IBasketRepository
// The code below adds a caching decorator for the `IBasketRepository` interface.
// It uses the `CachedBasketRepository` class to wrap the original `BasketRepository` implementation, adding caching functionality using `IDistributedCache`.
// using  scrutor library to decorate the `IBasketRepository` service with the `CachedBasketRepository` implementation. This allows for improved performance by caching basket data and reducing database calls.
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis")!;
});

// Add caching decorator for IBasketRepository 
/* The code below adds a caching decorator for the `IBasketRepository` interface. 
 * It uses the `CachedBasketRepository` class to wrap the original `BasketRepository` implementation, adding caching functionality using `IDistributedCache`.
 * This allows for improved performance by caching basket data and reducing database calls.*/
//builder.Services.AddScoped<IBasketRepository>(provider =>
//{
//    var basketRepository = provider.GetRequiredService<BasketRepository>();
//    return new CachedBasketRepository(basketRepository, provider.GetRequiredService<IDistributedCache>());
//});
// Add exception handler
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

// Add health checks 
/* The code below adds health checks to the application.
 * Health checks are used to monitor the health and availability of the application and its dependencies.*/
builder.Services.AddHealthChecks()
    // describes the health check for the PostgreSQL database connection. It uses the connection string from the configuration to check if the database is reachable and healthy.
    .AddNpgSql(builder.Configuration.GetConnectionString("BasketDb")!)
    // describes the health check for the Redis cache connection. It uses the connection string from the configuration to check if the Redis cache is reachable and healthy.
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapCarter();
// app.UseExceptionHandler(async exceptionHandler =>
app.UseExceptionHandler(options => { });
// the code below configures the application to use health checks and specifies the endpoint for health check requests.
app.UseHealthChecks("/health",
    // describes the options for the health check endpoint. It specifies that the response should be written in a format compatible with the HealthChecks UI.
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();

