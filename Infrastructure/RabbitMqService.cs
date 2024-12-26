using FlightManagementSystem.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlightManagementSystem.Infrastructure
{
    public interface IRabbitMqService
    {
        Task<FlightNotification?> ConsumeMessageAsync(CancellationToken cancellationToken);
        Task PublishMessageAsync(FlightNotification message);
        Task<uint> GetQueueSize(string queueName);
    }

    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly ILogger<RabbitMqService> _logger;
        private IConnection? _connection;
        private IChannel? _channel;
        private AsyncEventingBasicConsumer? _consumer;
        private string _consumerTag = string.Empty;
        private readonly ConcurrentQueue<FlightNotification> _messageQueue = new();

        public RabbitMqService(ILogger<RabbitMqService> logger, ConnectionFactory connectionFactory)
        {
            _logger = logger;
           
            EstablishRabbitMQConnection(connectionFactory);

            InitializeConsumer();
        }

        private void InitializeConsumer()
        {
            if (_consumer == null)
            {
                _consumer = new AsyncEventingBasicConsumer(_channel!);
                _consumer.ReceivedAsync += async (sender, e) =>
                {
                    try
                    {
                        var message = Encoding.UTF8.GetString(e.Body.Span);
                        var flightNotification = JsonSerializer.Deserialize<FlightNotification>(message);

                        if (flightNotification != null)
                        {
                            _messageQueue.Enqueue(flightNotification); // Add to the queue
                            await _channel!.BasicAckAsync(e.DeliveryTag, multiple: false);
                        }
                        else
                        {
                            await _channel!.BasicRejectAsync(e.DeliveryTag, requeue: false);
                        }
                    }
                    catch (Exception ex)
                    {
                        await _channel!.BasicRejectAsync(e.DeliveryTag, requeue: false);
                        _logger.LogError(ex, "Error processing message.");
                    }
                };

                // Start consuming messages
                _ = Task.Run(async () =>
                {
                    _consumerTag = await _channel!.BasicConsumeAsync(
                        queue: "FlightPricesQueue",
                        autoAck: false,
                        consumer: _consumer
                    );
                });
            }
        }
        private async void EstablishRabbitMQConnection(ConnectionFactory connectionFactory)
        {
            try
            {
                _connection = connectionFactory.CreateConnectionAsync().Result;
              
                _channel = _connection.CreateChannelAsync().Result;

                // Declare the queue once
                await _channel.QueueDeclareAsync(
                    queue: "FlightPricesQueue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                );

                // Set prefetch count to allow multiple messages at once
                await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

                _logger.LogInformation("RabbitMQ connection established and queue declared.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ. The service will continue without RabbitMQ.");
                _connection = null;
                _channel = null;
            }
        }

        public async Task PublishMessageAsync(FlightNotification flightNotification)
        {
            try
            {
                var message = JsonSerializer.Serialize(flightNotification);
             
                var body = Encoding.UTF8.GetBytes(message);

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
                throw;
            }
        }

        public async Task<FlightNotification?> ConsumeMessageAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var flightNotification))
                {
                    return flightNotification; // Return the first available message
                }

                // Delay to avoid busy waiting
                await Task.Delay(100, cancellationToken);
            }

            // Return null if the operation is canceled
            return null;
        }
        public async Task<uint> GetQueueSize(string queueName)
        {
            if (_channel == null)
            {
                throw new InvalidOperationException("Channel is not initialized.");
            }

            // Declare the queue passively to avoid modifying the queue
            var result = await _channel.QueueDeclarePassiveAsync(queueName);

            // The result will contain the queue's message count in the 'messageCount' property
            return result.MessageCount;
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
        }
    }


}
