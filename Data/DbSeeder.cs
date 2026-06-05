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

            try
            {
                context.Database.ExecuteSqlRaw("ALTER TABLE Testimonials ADD COLUMN Image TEXT NOT NULL DEFAULT ''");
            }
            catch (Exception)
            {
                // Column already exists, ignore
            }

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

        public static void MigrateFromSqliteToSqlServer(AppDbContext sqlServerContext)
        {
            var sqliteOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=bawadittamal.db")
                .Options;

            using var sqliteContext = new AppDbContext(sqliteOptions);

            // 1. Categories
            if (sqliteContext.Categories.Any())
            {
                sqlServerContext.Subcategories.RemoveRange(sqlServerContext.Subcategories);
                sqlServerContext.Categories.RemoveRange(sqlServerContext.Categories);
                sqlServerContext.SaveChanges();

                var sqliteCategories = sqliteContext.Categories.AsNoTracking().ToList();
                foreach (var cat in sqliteCategories)
                {
                    sqlServerContext.Categories.Add(new Category
                    {
                        Id = cat.Id,
                        Name = cat.Name,
                        Slug = cat.Slug,
                        Image = cat.Image,
                        Description = cat.Description
                    });
                }
                sqlServerContext.SaveChanges();
            }

            // 2. Subcategories (identity column)
            if (sqliteContext.Subcategories.Any())
            {
                var sqliteSubcats = sqliteContext.Subcategories.AsNoTracking().ToList();
                using (var transaction = sqlServerContext.Database.BeginTransaction())
                {
                    try
                    {
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Subcategories ON");
                        foreach (var sub in sqliteSubcats)
                        {
                            sqlServerContext.Subcategories.Add(new Subcategory
                            {
                                Id = sub.Id,
                                Name = sub.Name,
                                Slug = sub.Slug,
                                CategoryId = sub.CategoryId
                            });
                        }
                        sqlServerContext.SaveChanges();
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Subcategories OFF");
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // 3. Brands (identity column)
            if (sqliteContext.Brands.Any())
            {
                sqlServerContext.Brands.RemoveRange(sqlServerContext.Brands);
                sqlServerContext.SaveChanges();

                var sqliteBrands = sqliteContext.Brands.AsNoTracking().ToList();
                using (var transaction = sqlServerContext.Database.BeginTransaction())
                {
                    try
                    {
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Brands ON");
                        foreach (var brand in sqliteBrands)
                        {
                            sqlServerContext.Brands.Add(new Brand
                            {
                                Id = brand.Id,
                                Name = brand.Name,
                                Url = brand.Url,
                                Link = brand.Link
                            });
                        }
                        sqlServerContext.SaveChanges();
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Brands OFF");
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // 4. Products (string ID)
            if (sqliteContext.Products.Any())
            {
                sqlServerContext.Products.RemoveRange(sqlServerContext.Products);
                sqlServerContext.SaveChanges();

                var sqliteProducts = sqliteContext.Products.AsNoTracking().ToList();
                foreach (var prod in sqliteProducts)
                {
                    sqlServerContext.Products.Add(new Product
                    {
                        Id = prod.Id,
                        Slug = prod.Slug,
                        Name = prod.Name,
                        Brand = prod.Brand,
                        Category = prod.Category,
                        Subcategory = prod.Subcategory,
                        Type = prod.Type,
                        Price = prod.Price,
                        Mrp = prod.Mrp,
                        CatNo = prod.CatNo,
                        Status = prod.Status,
                        Stock = prod.Stock,
                        Description = prod.Description,
                        Images = prod.Images,
                        ShortSpecs = prod.ShortSpecs,
                        Specifications = prod.Specifications,
                        Variants = prod.Variants,
                        Features = prod.Features
                    });
                }
                sqlServerContext.SaveChanges();
            }

            // 5. Blogs (string ID)
            if (sqliteContext.Blogs.Any())
            {
                sqlServerContext.Blogs.RemoveRange(sqlServerContext.Blogs);
                sqlServerContext.SaveChanges();

                var sqliteBlogs = sqliteContext.Blogs.AsNoTracking().ToList();
                foreach (var blog in sqliteBlogs)
                {
                    sqlServerContext.Blogs.Add(new Blog
                    {
                        Id = blog.Id,
                        Title = blog.Title,
                        Slug = blog.Slug,
                        Image = blog.Image,
                        Category = blog.Category,
                        AuthorName = blog.AuthorName,
                        AuthorRole = blog.AuthorRole,
                        Tags = blog.Tags,
                        Excerpt = blog.Excerpt,
                        Content = blog.Content,
                        Date = blog.Date,
                        ReadTime = blog.ReadTime
                    });
                }
                sqlServerContext.SaveChanges();
            }

            // 6. GalleryItems (identity column)
            if (sqliteContext.GalleryItems.Any())
            {
                sqlServerContext.GalleryItems.RemoveRange(sqlServerContext.GalleryItems);
                sqlServerContext.SaveChanges();

                var sqliteGallery = sqliteContext.GalleryItems.AsNoTracking().ToList();
                using (var transaction = sqlServerContext.Database.BeginTransaction())
                {
                    try
                    {
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT GalleryItems ON");
                        foreach (var item in sqliteGallery)
                        {
                            sqlServerContext.GalleryItems.Add(new GalleryItem
                            {
                                Id = item.Id,
                                Title = item.Title,
                                Category = item.Category,
                                Image = item.Image
                            });
                        }
                        sqlServerContext.SaveChanges();
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT GalleryItems OFF");
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // 7. Testimonials (identity column)
            if (sqliteContext.Testimonials.Any())
            {
                sqlServerContext.Testimonials.RemoveRange(sqlServerContext.Testimonials);
                sqlServerContext.SaveChanges();

                var sqliteTestimonials = sqliteContext.Testimonials.AsNoTracking().ToList();
                using (var transaction = sqlServerContext.Database.BeginTransaction())
                {
                    try
                    {
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Testimonials ON");
                        foreach (var t in sqliteTestimonials)
                        {
                            sqlServerContext.Testimonials.Add(new Testimonial
                            {
                                Id = t.Id,
                                Name = t.Name,
                                Role = t.Role,
                                Company = t.Company,
                                Category = t.Category,
                                Quote = t.Quote,
                                Rating = t.Rating,
                                Status = t.Status,
                                Image = t.Image
                            });
                        }
                        sqlServerContext.SaveChanges();
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Testimonials OFF");
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // 8. SliderItems (identity column)
            if (sqliteContext.SliderItems.Any())
            {
                sqlServerContext.SliderItems.RemoveRange(sqlServerContext.SliderItems);
                sqlServerContext.SaveChanges();

                var sqliteSlider = sqliteContext.SliderItems.AsNoTracking().ToList();
                using (var transaction = sqlServerContext.Database.BeginTransaction())
                {
                    try
                    {
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT SliderItems ON");
                        foreach (var item in sqliteSlider)
                        {
                            sqlServerContext.SliderItems.Add(new SliderItem
                            {
                                Id = item.Id,
                                Image = item.Image,
                                Title = item.Title,
                                Subtitle = item.Subtitle,
                                Description = item.Description,
                                BtnText = item.BtnText,
                                BtnLink = item.BtnLink,
                                Btn1Text = item.Btn1Text,
                                Btn1Link = item.Btn1Link,
                                Btn2Text = item.Btn2Text,
                                Btn2Link = item.Btn2Link,
                                Status = item.Status
                            });
                        }
                        sqlServerContext.SaveChanges();
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT SliderItems OFF");
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // 9. Inquiries (identity column)
            if (sqliteContext.Inquiries.Any())
            {
                sqlServerContext.Inquiries.RemoveRange(sqlServerContext.Inquiries);
                sqlServerContext.SaveChanges();

                var sqliteInquiries = sqliteContext.Inquiries.AsNoTracking().ToList();
                using (var transaction = sqlServerContext.Database.BeginTransaction())
                {
                    try
                    {
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Inquiries ON");
                        foreach (var inq in sqliteInquiries)
                        {
                            sqlServerContext.Inquiries.Add(new Inquiry
                            {
                                Id = inq.Id,
                                Name = inq.Name,
                                Email = inq.Email,
                                Phone = inq.Phone,
                                Subject = inq.Subject,
                                Message = inq.Message,
                                Date = inq.Date,
                                Status = inq.Status
                            });
                        }
                        sqlServerContext.SaveChanges();
                        sqlServerContext.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Inquiries OFF");
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            // 10. Settings (string key ID)
            if (sqliteContext.Settings.Any())
            {
                sqlServerContext.Settings.RemoveRange(sqlServerContext.Settings);
                sqlServerContext.SaveChanges();

                var sqliteSettings = sqliteContext.Settings.AsNoTracking().ToList();
                foreach (var set in sqliteSettings)
                {
                    sqlServerContext.Settings.Add(new Setting
                    {
                        Key = set.Key,
                        Value = set.Value
                    });
                }
                sqlServerContext.SaveChanges();
            }
        }
    }
}
