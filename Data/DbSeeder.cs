using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Models;

namespace BawaDittaMal.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();


            if (context.Categories.Any())
            {
                return; // Database is already seeded
            }

            try
            {
                var jsonDir = FindJsonDirectory();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 1. Seed Categories & Subcategories
                SeedCategories(context, jsonDir, jsonOptions);

                // 2. Seed Brands
                SeedBrands(context, jsonDir, jsonOptions);

                // 3. Seed Gallery
                SeedGallery(context, jsonDir, jsonOptions);

                // 4. Seed Products
                SeedProducts(context, jsonDir, jsonOptions);

                // 5. Seed Site Content (Testimonials, Slider, Settings, Blogs)
                SeedSiteContent(context, jsonDir, jsonOptions);

                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding database: {ex.Message}");
            }
        }

        private static string FindJsonDirectory()
        {
            var baseDir = AppContext.BaseDirectory;
            string[] pathsToTry = {
                Path.Combine(baseDir, "..", "..", "..", "..", "bawadittamal-frontend", "src", "data"),
                Path.Combine(baseDir, "..", "..", "..", "bawadittamal-frontend", "src", "data"),
                Path.Combine(baseDir, "..", "bawadittamal-frontend", "src", "data"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "bawadittamal-frontend", "src", "data"),
                @"d:\Aditya\bava-datta\bawadittamal-frontend\src\data"
            };

            foreach (var path in pathsToTry)
            {
                if (Directory.Exists(path))
                {
                    Console.WriteLine($"Found JSON directory at: {path}");
                    return path;
                }
            }
            throw new DirectoryNotFoundException("Could not find frontend data directory.");
        }

        private static void SeedCategories(AppDbContext context, string jsonDir, JsonSerializerOptions options)
        {
            var filePath = Path.Combine(jsonDir, "categories.json");
            if (!File.Exists(filePath)) return;

            var jsonString = File.ReadAllText(filePath);
            var categories = JsonSerializer.Deserialize<List<Category>>(jsonString, options);

            if (categories != null)
            {
                foreach (var cat in categories)
                {
                    // EF Core will save subcategories correctly because of Category -> Subcategories relation
                    // Set foreign key explicit values
                    foreach (var sub in cat.Subcategories)
                    {
                        sub.CategoryId = cat.Id;
                    }
                    context.Categories.Add(cat);
                }
            }
        }

        private static void SeedBrands(AppDbContext context, string jsonDir, JsonSerializerOptions options)
        {
            var filePath = Path.Combine(jsonDir, "brands.json");
            if (!File.Exists(filePath)) return;

            var jsonString = File.ReadAllText(filePath);
            var brands = JsonSerializer.Deserialize<List<Brand>>(jsonString, options);
            if (brands != null)
            {
                context.Brands.AddRange(brands);
            }
        }

        private static void SeedGallery(AppDbContext context, string jsonDir, JsonSerializerOptions options)
        {
            var filePath = Path.Combine(jsonDir, "gallery.json");
            if (!File.Exists(filePath)) return;

            var jsonString = File.ReadAllText(filePath);
            var galleryItems = JsonSerializer.Deserialize<List<GalleryItem>>(jsonString, options);
            if (galleryItems != null)
            {
                context.GalleryItems.AddRange(galleryItems);
            }
        }

        private static void SeedProducts(AppDbContext context, string jsonDir, JsonSerializerOptions options)
        {
            var filePath = Path.Combine(jsonDir, "products.json");
            if (!File.Exists(filePath)) return;

            var jsonString = File.ReadAllText(filePath);
            var products = JsonSerializer.Deserialize<List<Product>>(jsonString, options);
            if (products != null)
            {
                context.Products.AddRange(products);
            }
        }

        private static void SeedSiteContent(AppDbContext context, string jsonDir, JsonSerializerOptions options)
        {
            var filePath = Path.Combine(jsonDir, "site-content.json");
            if (!File.Exists(filePath)) return;

            var jsonString = File.ReadAllText(filePath);
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            // Seed Testimonials
            if (root.TryGetProperty("homePage", out var homePage) &&
                homePage.TryGetProperty("testimonials", out var testimonialsSection) &&
                testimonialsSection.TryGetProperty("list", out var listElement))
            {
                var testimonials = JsonSerializer.Deserialize<List<Testimonial>>(listElement.GetRawText(), options);
                if (testimonials != null)
                {
                    foreach (var t in testimonials)
                    {
                        if (t.Role.Contains(","))
                        {
                            var parts = t.Role.Split(',');
                            t.Role = parts[0].Trim();
                            t.Company = parts[1].Trim();
                        }
                        else
                        {
                            t.Company = "BDM Customer";
                        }
                        t.Category = "Tiles & Marbles";
                        t.Status = "active";
                    }
                    context.Testimonials.AddRange(testimonials);
                }
            }

            // Seed Slider Items
            if (homePage.TryGetProperty("heroSlider", out var sliderElement))
            {
                var sliderItems = JsonSerializer.Deserialize<List<SliderItem>>(sliderElement.GetRawText(), options);
                if (sliderItems != null)
                {
                    context.SliderItems.AddRange(sliderItems);
                }
            }

            // Seed Settings (Contact Info)
            if (root.TryGetProperty("common", out var common) &&
                common.TryGetProperty("contact", out var contactElement))
            {
                context.Settings.Add(new Setting
                {
                    Key = "contact",
                    Value = contactElement.GetRawText()
                });
            }

            // Seed Settings (Social Links)
            if (root.TryGetProperty("footer", out var footer) &&
                footer.TryGetProperty("socialLinks", out var socialElement))
            {
                context.Settings.Add(new Setting
                {
                    Key = "social",
                    Value = socialElement.GetRawText()
                });
            }

            // Seed default blogs
            var defaultBlog = new Blog
            {
                Id = "BLOG-001",
                Image = "/hero-1.png",
                Title = "How to Choose the Perfect Bathroom Fittings",
                Slug = "choosing-perfect-bathroom-fittings",
                Category = "Sanitaryware",
                AuthorName = "Admin",
                AuthorRole = "BDM Galleria Admin",
                Tags = "Sanitary, Luxury, Jaquar",
                Excerpt = "Discover the key factors to consider when selecting premium sanitaryware and bathroom fittings to elevate your home design.",
                Content = "Bathroom fittings play a major role in redefining luxury and comfort. Sourcing global brand fittings ensures both durability and aesthetic harmony. Jaquar is one such partner that delivers technological excellence and design elegance.",
                Date = "May 25, 2026",
                ReadTime = "3 Min Read"
            };
            context.Blogs.Add(defaultBlog);
        }
    }
}
