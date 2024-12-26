using FlightManagementSystem.Infrastructure;
using FlightManagementSystem.Models;

namespace FlightManagementSystem.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IUserService _userService;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IPushNotificationService _pushNotificationService;

        public NotificationService(IUserService userService, IRabbitMqService rabbitMqService, IPushNotificationService pushNotificationService)
        {
            _userService = userService;
            _rabbitMqService = rabbitMqService;
            _pushNotificationService = pushNotificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Pull messages from RabbitMQ in batches
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
            var relevantUsers = users.Where(user => user.AlertPreferences.Any(preference => preference.MaxPrice >= flightMessage.Price && preference.Destination == flightMessage.Destination)).ToList();

            // Send notifications to these clients
            foreach (var user in relevantUsers)
            {
                SendNotificationAsync(user, flightMessage);
            }
        }

        private void SendNotificationAsync(User user, FlightNotification flightMessage)
        {
            var notificationMessage = $"Hi {user.Name}, the flight '{flightMessage.Airline}' from {flightMessage.Origin} to {flightMessage.Destination} is now available for {flightMessage.Price} {flightMessage.Currency}.";

            _pushNotificationService.SendPushAlert(notificationMessage);
        }
    }
}
