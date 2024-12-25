using FlightManagementSystem.Infrastructure;
using FlightManagementSystem.Services;
using MongoDB.Driver;
using NLog;
using NLog.Web;
using RabbitMQ.Client;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Debug("Initializing application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSingleton<IFlightPriceService, FlightPriceService>();
    builder.Services.AddSingleton<INotificationService, NotificationService>();
    builder.Services.AddSingleton<IUserService, UserService>();
    builder.Services.AddSingleton<IUserRepository, UserRepository>();
    builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();

    builder.Services.AddHttpClient(); 
    builder.Services.AddHostedService<FlightPriceService>();

    builder.Services.AddSingleton<RabbitMqService>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILogger<RabbitMqService>>();

        // Configure RabbitMQ from appsettings.json
        var rabbitMqConfig = configuration.GetSection("RabbitMQ");
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqConfig!["HostName"]!,
            UserName = rabbitMqConfig!["UserName"]!,
            Password = rabbitMqConfig!["Password"]!,
            Port = int.TryParse(rabbitMqConfig["Port"], out var port) ? port : 5672,
            VirtualHost = rabbitMqConfig["VirtualHost"] ?? "/"
        };

        return new RabbitMqService(logger, factory);
    });


    builder.Logging.ClearProviders();

    builder.Host.UseNLog();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              );
    });


    builder.Services.AddControllers();

    builder.Services.AddSingleton<IMongoClient>(sp =>
    {
        return new MongoClient("mongodb://localhost:27017");
    });

    var app = builder.Build();

    app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

    app.MapControllers();

    app.MapGet("/", () => "Hello from Flight Management System!");

    await app.RunAsync();
}
catch(Exception exception)
{
    logger.Error(exception, "Application terminated unexpectedly.");
    throw;
}
finally
{
    LogManager.Shutdown();
}