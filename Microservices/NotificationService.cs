namespace FlightManagementSystem.Services
{
    public interface INotificationService
    {

    }

    public class NotificationService : INotificationService
    {
        private readonly IUserService _userManagementService;
        public NotificationService(IUserService userManagementService)
        {
            _userManagementService = userManagementService;
        }
    }
}
