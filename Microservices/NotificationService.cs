using FlightManagementSystem.Infrastructure;
using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services
{
    public interface INotificationService
    {
        // Task ProcessNotificationAsync(FlightNotification flightMessage);
    }

    public class NotificationService : BackgroundService, INotificationService
    {
        private readonly IUserService _userService;
        private readonly RabbitMqService _rabbitMqService;
        private readonly IPushNotificationService _pushNotificationService;

        public NotificationService(IUserService userService, RabbitMqService rabbitMqService, IPushNotificationService pushNotificationService)
        {
            _userService = userService;
            _rabbitMqService = rabbitMqService;
            _pushNotificationService = pushNotificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Pull messages from RabbitMQ
                var flightMessage = await _rabbitMqService.ConsumeMessageAsync(stoppingToken);
          
                if (flightMessage != null)
                {
                    await ProcessNotificationAsync(flightMessage);
                }

                // Add a small delay to prevent excessive CPU usage
                await Task.Delay(500, stoppingToken);
            }
        }

        // This method filters the clients based on their max price and the flight price
        private async Task ProcessNotificationAsync(FlightNotification flightMessage)
        {
            // Fetch all clients from the IUserService
            var users = await _userService.GetAllUsersAsync();

            // Filter clients based on their alert preferences and flight price
            var relevantUsers = users.Where(user => user.AlertPreferences.Any(preference => preference.MaxPrice >= flightMessage.Price)).ToList();

            // Send notifications to these clients
            foreach (var user in relevantUsers)
            {
                await SendNotificationAsync(user, flightMessage);
            }
        }

        // This method sends a notification to the client (you can implement your own notification mechanism here)
        private async Task SendNotificationAsync(User user, FlightNotification flightMessage)
        {
            // Construct the notification message
            var notificationMessage = $"Hi {user.Name}, the flight '{flightMessage.Airline}' from {flightMessage.Origin} to {flightMessage.Destination} is now available for {flightMessage.Price} {flightMessage.Currency}.";

            // Use the push notification service to send the alert
            _pushNotificationService.SendPushAlert(notificationMessage);

            // Simulate sending notification delay
            await Task.Delay(500);
        }

    }
}
