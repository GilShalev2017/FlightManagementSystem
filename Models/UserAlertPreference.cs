namespace FlightManagementSystem.Models
{
    public class UserAlertPreference
    {
        public string? PreferenceId { get; set; }
        public required string Destination { get; set; }
        public required decimal MaxPrice { get; set; }
        public required string Currency { get; set; }
    }
}
