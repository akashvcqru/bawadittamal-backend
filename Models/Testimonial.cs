namespace BawaDittaMal.Api.Models
{
    public class Testimonial
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Quote { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public string Status { get; set; } = "active"; // active, inactive
    }
}
