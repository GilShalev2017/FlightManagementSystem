namespace FlightManagementSystem.Infrastructure
{
    public interface IPushNotificationService
    {
        void SendPushAlert(string message);
    }

    public class PushNotificationService : IPushNotificationService
    {
        public void SendPushAlert(string message)
        {
            // Fake/demo implementation: print that the push alert was sent
            Console.WriteLine($"Push alert sent: {message}");
        }
    }

}
