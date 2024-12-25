using RabbitMQ.Client;
using static MongoDB.Driver.WriteConcern;
using System.Text;
using FlightManagementSystem.Services;
using System.Threading.Channels;

namespace FlightManagementSystem.Infrastructure
{
    public class RabbitMqService
    {
        private readonly ILogger<RabbitMqService> _logger;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqService(ILogger<RabbitMqService> logger, ConnectionFactory connectionFactory)
        {
            _logger = logger;

            EstablishRabbitMQConnection(connectionFactory);
        }

        private void EstablishRabbitMQConnection(ConnectionFactory connectionFactory)
        {
            try
            {
                _connection = connectionFactory.CreateConnectionAsync().Result;
              
                _channel = _connection.CreateChannelAsync().Result;

                // Declare the queue once
                _channel.QueueDeclareAsync(
                    queue: "FlightPricesQueue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                _logger.LogInformation("RabbitMQ connection established and queue declared.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ. The service will continue without RabbitMQ.");
                // Set _connection and _channel to null to indicate RabbitMQ is unavailable
                _connection = null;
                _channel = null;
            }
        }

        public async Task PublishMessageAsync(byte[] body)
        {
            try
            {
                // Publish to RabbitMQ
                await _channel!.BasicPublishAsync(
                    exchange: "",
                    routingKey: "FlightPricesQueue",
                    mandatory: false,
                    body: body
                );

                _logger.LogInformation("Message published to RabbitMQ successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to RabbitMQ.");
                throw; // Re-throw to let the caller handle it if necessary
            }
        }


        public async void Dispose()
        {
            await _channel!.CloseAsync();
            await _connection!.CloseAsync();
        }
    }

}
