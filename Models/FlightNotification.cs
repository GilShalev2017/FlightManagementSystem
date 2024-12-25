namespace FlightManagementSystem.Models
{
    public class FlightNotification
    {
        public required string FlightId { get; set; }
        public required string Airline { get; set; }
        public required string Origin { get; set; }
        public required string Destination { get; set; }
        public required DateTime DepartureDate { get; set; }
        public required decimal Price { get; set; }
        public required string Currency { get; set; }
    }
}
