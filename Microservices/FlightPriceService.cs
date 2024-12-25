
using FlightManagementSystem.Infrastructure;
using FlightManagementSystem.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace FlightManagementSystem.Services
{
    public interface IFlightPriceService
    {
        Task FetchFlightPricesAsync(CancellationToken cancellationToken);
    }

    public class FlightPriceService : BackgroundService, IFlightPriceService
    {
        private readonly ILogger<FlightPriceService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly List<string> _apiEndpoints;
        private readonly IRabbitMqService _rabbitMqService;

        public FlightPriceService(ILogger<FlightPriceService> logger, IConfiguration configuration, 
            IHttpClientFactory httpClientFactory, IRabbitMqService rabbitMqService)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _apiEndpoints = configuration.GetSection("FlightAPIs").Get<List<string>>() ?? new List<string>();
            _rabbitMqService = rabbitMqService;
        }

        // BackgroundService: Executes periodically
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Flight Price Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await FetchFlightPricesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while fetching flight prices.");
                }

                // Wait before the next fetch (e.g., every 10 minutes)
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("Flight Price Service is stopping.");
        }

        // Fetch flight prices from external APIs
        public async Task FetchFlightPricesAsync(CancellationToken cancellationToken)
        {
            if (!_apiEndpoints.Any())
            {
                _logger.LogWarning("No flight API endpoints available for fetching.");
                return;
            }

            var httpClient = _httpClientFactory.CreateClient();

            foreach (var endpoint in _apiEndpoints)
            {
                try
                {
                    _logger.LogInformation($"Fetching flight prices from {endpoint}...");
                    var response = await httpClient.GetAsync(endpoint, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                        var flightPrices = JsonSerializer.Deserialize<List<FlightNotification>>(jsonResponse);

                        // Publish each flight price to RabbitMQ
                        foreach (var price in flightPrices ?? new List<FlightNotification>())
                        {
                            await PublishToQueueAsync(price);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to fetch data from {endpoint}. Status: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error fetching data from {endpoint}");
                }
            }
        }

        private async Task PublishToQueueAsync(FlightNotification price)
        {
            try
            {
                // Publish message
                await _rabbitMqService.PublishMessageAsync(price);

                _logger.LogInformation($"Published flight price to queue: {price}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish flight price to queue.");
            }
        }

    }
}
