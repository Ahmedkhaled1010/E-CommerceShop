

using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

//Add Service to the container
var assemply = typeof(Program).Assembly;
builder.Services.AddCarter();
builder.Services.AddMediatR(cfg =>
  {
      cfg.RegisterServicesFromAssembly(assemply);
      cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
      cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
  });
builder.Services.AddValidatorsFromAssembly(assemply);
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("CatalogDb")!);
}).UseLightweightSessions();
if (builder.Environment.IsDevelopment())
{
    builder.Services.InitializeMartenWith<CatalogInitialData>();
}
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("CatalogDb")!);
var app = builder.Build();

//Configure the HTTP request pipeline
app.MapCarter();

//app.UseExceptionHandler(async exceptionHandler =>

//{
//    exceptionHandler.Run(async context =>
//    {
//        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
//        if (exception is null)
//        {
//            return;
//        }
//        var problemDetails = new ProblemDetails
//        {
//            Title = exception.Message,
//            Status = StatusCodes.Status500InternalServerError,
//            Detail = exception.StackTrace
//        };
//        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//        logger.LogError(exception, exception.Message);
//        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//        context.Response.ContentType = "application/problem+json";
//        await context.Response.WriteAsJsonAsync(problemDetails);
//    });

//});
app.UseExceptionHandler(options => { });
app.UseHealthChecks("/health",
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        //ResponseWriter = async (context, report) =>
        //{
        //    context.Response.ContentType = "application/json";
        //    var response = new
        //    {
        //        status = report.Status.ToString(),
        //        checks = report.Entries.Select(e => new
        //        {
        //            name = e.Key,
        //            status = e.Value.Status.ToString(),
        //            exception = e.Value.Exception?.Message,
        //            duration = e.Value.Duration.ToString()
        //        })
        //    };
        //    await context.Response.WriteAsJsonAsync(response);
        //}
    }
    );
app.Run();
