using System.Collections.Generic;

namespace BawaDittaMal.Api.Models
{
    public class Category
    {
        public string Id { get; set; } = string.Empty; // e.g. "plywood-glass"
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Subcategory> Subcategories { get; set; } = new();
    }

    public class Subcategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
    }
}
