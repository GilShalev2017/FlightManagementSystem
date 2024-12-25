using FlightManagementSystem.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FlightManagementSystem.Infrastructure
{
    public interface IRabbitMqService
    {
        Task<FlightNotification> ConsumeMessageAsync(CancellationToken cancellationToken);
        Task PublishMessageAsync(FlightNotification message);
    }

    public class RabbitMqService : IRabbitMqService
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

        public async Task PublishMessageAsync(FlightNotification price)
        {
            try
            {
                // Serialize the message
                var message = JsonSerializer.Serialize(price);

                var body = Encoding.UTF8.GetBytes(message);

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
            // Use AsyncEventingBasicConsumer for async operations
            var consumer = new AsyncEventingBasicConsumer(_channel!);

            var taskCompletionSource = new TaskCompletionSource<FlightNotification>();

            // Use async event handler for ReceivedAsync
            consumer.ReceivedAsync += async (sender, e) =>
            {
                try
                {
                    // Deserialize the message from the queue
                    var message = Encoding.UTF8.GetString(e.Body.Span);

                    var flightNotification = JsonSerializer.Deserialize<FlightNotification>(message);

                    if (flightNotification != null)
                    {
                        // Set the result when the message is processed successfully
                        taskCompletionSource.SetResult(flightNotification);

                        // Acknowledge the message to remove it from the queue
                        await _channel!.BasicAckAsync(deliveryTag: e.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        // Ignore messages that are not of type FlightNotification
                        await _channel!.BasicRejectAsync(deliveryTag: e.DeliveryTag, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error and reject the message to avoid reprocessing
                    await _channel!.BasicRejectAsync(deliveryTag: e.DeliveryTag, requeue: false);
                   
                    taskCompletionSource.SetException(ex);
                }

                // Return Task.CompletedTask to indicate completion of async operation
                await Task.CompletedTask;
            };

            // Start consuming the messages asynchronously
            await _channel!.BasicConsumeAsync(queue: "FlightPricesQueue", autoAck: false, consumer: consumer);

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
