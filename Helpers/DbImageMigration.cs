using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using BawaDittaMal.Api.Data;
using BawaDittaMal.Api.Models;

namespace BawaDittaMal.Api.Helpers
{
    public static class DbImageMigration
    {
        public static async Task MigrateBase64Images(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            string rootPath = env.ContentRootPath;

            try
            {
                // 1. Brands
                var brands = await context.Brands.ToListAsync();
                bool brandsChanged = false;
                foreach (var b in brands)
                {
                    if (IsBase64(b.Url))
                    {
                        b.Url = FileHelper.SaveBase64Image(b.Url, rootPath, "brand");
                        brandsChanged = true;
                    }
                }

                // 2. SliderItems
                var sliders = await context.SliderItems.ToListAsync();
                bool slidersChanged = false;
                foreach (var s in sliders)
                {
                    if (IsBase64(s.Image))
                    {
                        s.Image = FileHelper.SaveBase64Image(s.Image, rootPath, "slider");
                        slidersChanged = true;
                    }
                }

                // 3. GalleryItems
                var gallery = await context.GalleryItems.ToListAsync();
                bool galleryChanged = false;
                foreach (var g in gallery)
                {
                    if (IsBase64(g.Image))
                    {
                        g.Image = FileHelper.SaveBase64Image(g.Image, rootPath, "gallery");
                        galleryChanged = true;
                    }
                }

                // 4. Products
                var products = await context.Products.ToListAsync();
                bool productsChanged = false;
                foreach (var p in products)
                {
                    if (p.Images != null && p.Images.Any(IsBase64))
                    {
                        p.Images = p.Images.Select(img => FileHelper.SaveBase64Image(img, rootPath, "product")).ToList();
                        productsChanged = true;
                        // Force EF Core to recognize change for serialized JSON property
                        context.Entry(p).Property(x => x.Images).IsModified = true;
                    }
                }

                // 5. Categories
                var categories = await context.Categories.ToListAsync();
                bool categoriesChanged = false;
                foreach (var c in categories)
                {
                    if (IsBase64(c.Image))
                    {
                        c.Image = FileHelper.SaveBase64Image(c.Image, rootPath, "category");
                        categoriesChanged = true;
                    }
                }

                // 6. Blogs
                var blogs = await context.Blogs.ToListAsync();
                bool blogsChanged = false;
                foreach (var bl in blogs)
                {
                    if (IsBase64(bl.Image))
                    {
                        bl.Image = FileHelper.SaveBase64Image(bl.Image, rootPath, "blog");
                        blogsChanged = true;
                    }
                }

                // 7. Testimonials
                var testimonials = await context.Testimonials.ToListAsync();
                bool testimonialsChanged = false;
                foreach (var t in testimonials)
                {
                    if (IsBase64(t.Image))
                    {
                        t.Image = FileHelper.SaveBase64Image(t.Image, rootPath, "testimonial");
                        testimonialsChanged = true;
                    }
                }

                if (brandsChanged || slidersChanged || galleryChanged || productsChanged || categoriesChanged || blogsChanged || testimonialsChanged)
                {
                    Console.WriteLine("[Migration] Found base64 images in database. Saving them as files and updating database entries...");
                    await context.SaveChangesAsync();
                    Console.WriteLine("[Migration] Database entries updated successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Migration] Error during image migration: {ex.Message}");
            }
        }

        private static bool IsBase64(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) && value.Contains(";base64,");
        }
    }
}
