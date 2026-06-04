using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;
using BawaDittaMal.Api.Models;

namespace BawaDittaMal.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Subcategory> Subcategories { get; set; } = null!;
        public DbSet<Brand> Brands { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<GalleryItem> GalleryItems { get; set; } = null!;
        public DbSet<Testimonial> Testimonials { get; set; } = null!;
        public DbSet<SliderItem> SliderItems { get; set; } = null!;
        public DbSet<Inquiry> Inquiries { get; set; } = null!;
        public DbSet<Setting> Settings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Primary Keys
            modelBuilder.Entity<Category>().HasKey(c => c.Id);
            modelBuilder.Entity<Setting>().HasKey(s => s.Key);
            modelBuilder.Entity<Product>().HasKey(p => p.Id);

            // Configure value converters for JSON columns (SQLite doesn't natively support array types)
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            modelBuilder.Entity<Product>()
                .Property(p => p.Images)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
                );

            modelBuilder.Entity<Product>()
                .Property(p => p.ShortSpecs)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
                );

            modelBuilder.Entity<Product>()
                .Property(p => p.Specifications)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<ProductSpecification>>(v, jsonOptions) ?? new List<ProductSpecification>()
                );

            modelBuilder.Entity<Product>()
                .Property(p => p.Variants)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<ProductVariant>>(v, jsonOptions) ?? new List<ProductVariant>()
                );

            modelBuilder.Entity<Product>()
                .Property(p => p.Features)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<ProductFeature>>(v, jsonOptions) ?? new List<ProductFeature>()
                );
        }
    }
}
