namespace BawaDittaMal.Api.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty; // Image url
        public string Link { get; set; } = string.Empty; // Slug/Link
    }
}
