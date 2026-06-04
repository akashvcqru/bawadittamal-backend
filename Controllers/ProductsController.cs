using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Data;
using BawaDittaMal.Api.Models;
using BawaDittaMal.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BawaDittaMal.Api.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedData<Product>>>> GetProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? category = null,
            [FromQuery] string? subcategory = null,
            [FromQuery] string? brand = null,
            [FromQuery] string? status = null)
        {
            var query = _context.Products.AsQueryable();

            // Search filter (handles ID, name, brand, category, description)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(cleanSearch) ||
                    p.Id.ToLower().Contains(cleanSearch) ||
                    p.Brand.ToLower().Contains(cleanSearch) ||
                    p.Category.ToLower().Contains(cleanSearch) ||
                    p.Description.ToLower().Contains(cleanSearch)
                );
            }

            // Category filter (supports either slug or category name)
            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", System.StringComparison.OrdinalIgnoreCase))
            {
                var cleanCategory = category.Trim().ToLower();
                // Check match against both the Category field (slug/id) or the category details name if matches
                query = query.Where(p => p.Category.ToLower() == cleanCategory || p.Category.Replace("-", " ").ToLower() == cleanCategory);
            }

            // Subcategory filter (supports either slug or subcategory name)
            if (!string.IsNullOrWhiteSpace(subcategory) && !subcategory.Equals("All", System.StringComparison.OrdinalIgnoreCase))
            {
                var cleanSubcategory = subcategory.Trim().ToLower();
                query = query.Where(p => p.Subcategory.ToLower() == cleanSubcategory || p.Subcategory.Replace("-", " ").ToLower() == cleanSubcategory);
            }

            // Brand filter
            if (!string.IsNullOrWhiteSpace(brand))
            {
                var cleanBrand = brand.Trim().ToLower();
                query = query.Where(p => p.Brand.ToLower() == cleanBrand);
            }

            // Status filter (e.g. In Stock, Out of Stock, Low Stock)
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", System.StringComparison.OrdinalIgnoreCase))
            {
                var cleanStatus = status.Trim().ToLower();
                query = query.Where(p => p.Status.ToLower() == cleanStatus);
            }

            // Order products: newest or most relevant first, default by Id desc/name
            query = query.OrderBy(p => p.Id);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedData = new PaginatedData<Product>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Success(paginatedData, "Products fetched successfully.");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Product>>> GetProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                // Try finding by slug
                product = await _context.Products.FirstOrDefaultAsync(p => p.Slug == id);
                if (product == null)
                {
                    return Error<Product>("Product not found.", 404);
                }
            }
            return Success(product, "Product fetched successfully.");
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Product>>> CreateProduct([FromBody] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Id))
            {
                product.Id = System.Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            }

            if (string.IsNullOrWhiteSpace(product.Slug))
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-").Replace("/", "-");
            }

            if (await _context.Products.AnyAsync(p => p.Id == product.Id))
            {
                return Error<Product>("Product with this ID already exists.");
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Success(product, "Product created successfully.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Product>>> UpdateProduct(string id, [FromBody] Product productData)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Error<Product>("Product not found.", 404);
            }

            product.Name = productData.Name;
            product.Slug = productData.Slug;
            product.Brand = productData.Brand;
            product.Category = productData.Category;
            product.Subcategory = productData.Subcategory;
            product.Type = productData.Type;
            product.Price = productData.Price;
            product.Mrp = productData.Mrp;
            product.CatNo = productData.CatNo;
            product.Status = productData.Status;
            product.Stock = productData.Stock;
            product.Description = productData.Description;
            product.Images = productData.Images;
            product.ShortSpecs = productData.ShortSpecs;
            product.Specifications = productData.Specifications;
            product.Variants = productData.Variants;
            product.Features = productData.Features;

            await _context.SaveChangesAsync();
            return Success(product, "Product updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Error<string>("Product not found.", 404);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Success(id, "Product deleted successfully.");
        }
    }
}
