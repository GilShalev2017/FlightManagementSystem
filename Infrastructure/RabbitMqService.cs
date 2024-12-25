using FlightManagementSystem.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

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

        public async Task<FlightNotification> ConsumeMessageAsync(CancellationToken cancellationToken)
        {
            // Use AsyncEventingBasicConsumer instead of EventingBasicConsumer
            var consumer = new AsyncEventingBasicConsumer(_channel!);

            var taskCompletionSource = new TaskCompletionSource<FlightNotification>();

            // Use async event handler for ReceivedAsync
            consumer.ReceivedAsync += (sender, e) =>
            {
                // Deserialize the message from the queue
                var message = Encoding.UTF8.GetString(e.Body.Span);

                var flightNotification = JsonSerializer.Deserialize<FlightNotification>(message);

                if (flightNotification != null)
                {
                    // Set the result when the message is processed
                    taskCompletionSource.SetResult(flightNotification);
                }
                else
                {
                    // Handle the case where flightNotification is null
                    taskCompletionSource.SetException(new Exception("Failed to deserialize the message."));
                }

                // Return Task.CompletedTask to indicate completion of async operation without returning a value
                return Task.CompletedTask;
            };

            // Start consuming the messages asynchronously
            await _channel!.BasicConsumeAsync(queue: "FlightPricesQueue", autoAck: true, consumer: consumer);

            // Wait until the message is processed
            return await taskCompletionSource.Task;
        }

        public async void Dispose()
        {
            await _channel!.CloseAsync();
            await _connection!.CloseAsync();
        }
    }

}
