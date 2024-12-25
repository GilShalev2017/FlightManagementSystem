namespace FlightManagementSystem.Infrastructure
{
    public interface IPushNotificationService
    {
        void SendPushAlert(string message);
    }

    public class PushNotificationService : IPushNotificationService
    {
        ILogger<PushNotificationService> _logger;

        public PushNotificationService(ILogger<PushNotificationService> logger)
        {
            _logger = logger;
        }

        public void SendPushAlert(string message)
        {
            // Fake/demo implementation: print that the push alert was sent
            _logger.LogInformation($"Push alert sent: {message}");
        }
    }

}
