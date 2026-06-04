using System.Collections.Generic;

namespace BawaDittaMal.Api.Models
{
    public class Product
    {
        public string Id { get; set; } = string.Empty; // e.g. "charm-6d" or "PROD-001"
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Category slug
        public string Subcategory { get; set; } = string.Empty; // Subcategory slug
        public string Type { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Mrp { get; set; } = string.Empty;
        public string CatNo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // e.g. "In Stock", "Out of Stock"
        public int Stock { get; set; }
        public string Description { get; set; } = string.Empty;

        // Serialized collections for SQLite storage
        public List<string> Images { get; set; } = new();
        public List<string> ShortSpecs { get; set; } = new();
        public List<ProductSpecification> Specifications { get; set; } = new();
        public List<ProductVariant> Variants { get; set; } = new();
        public List<ProductFeature> Features { get; set; } = new();
    }

    public class ProductSpecification
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ProductVariant
    {
        public string Label { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
    }

    public class ProductFeature
    {
        public string Title { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
    }
}
