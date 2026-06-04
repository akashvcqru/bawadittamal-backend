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
    public class CategoriesController : BaseApiController
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Category>>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Subcategories)
                .ToListAsync();
            return Success(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Category>>> GetCategory(string id)
        {
            var category = await _context.Categories
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return Error<Category>("Category not found.", 404);
            }

            return Success(category);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Category>>> CreateCategory([FromBody] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Id))
            {
                category.Id = category.Slug;
            }

            if (await _context.Categories.AnyAsync(c => c.Id == category.Id))
            {
                return Error<Category>("Category with this ID already exists.");
            }

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return Success(category, "Category created successfully.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Category>>> UpdateCategory(string id, [FromBody] Category categoryData)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return Error<Category>("Category not found.", 404);
            }

            category.Name = categoryData.Name;
            category.Slug = categoryData.Slug;
            category.Image = categoryData.Image;
            category.Description = categoryData.Description;

            await _context.SaveChangesAsync();
            return Success(category, "Category updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteCategory(string id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return Error<string>("Category not found.", 404);
            }

            _context.Categories.Remove(category);
            
            // Delete related subcategories
            var subcategories = _context.Subcategories.Where(s => s.CategoryId == id);
            _context.Subcategories.RemoveRange(subcategories);

            await _context.SaveChangesAsync();
            return Success(id, "Category and its subcategories deleted successfully.");
        }

        // Subcategory endpoints
        [HttpPost("subcategories")]
        public async Task<ActionResult<ApiResponse<Subcategory>>> CreateSubcategory([FromBody] Subcategory subcategory)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == subcategory.CategoryId))
            {
                return Error<Subcategory>("Parent category not found.");
            }

            _context.Subcategories.Add(subcategory);
            await _context.SaveChangesAsync();

            return Success(subcategory, "Subcategory added successfully.");
        }

        [HttpPut("subcategories/{id}")]
        public async Task<ActionResult<ApiResponse<Subcategory>>> UpdateSubcategory(int id, [FromBody] Subcategory subcategoryData)
        {
            var subcategory = await _context.Subcategories.FindAsync(id);
            if (subcategory == null)
            {
                return Error<Subcategory>("Subcategory not found.", 404);
            }

            subcategory.Name = subcategoryData.Name;
            subcategory.Slug = subcategoryData.Slug;
            subcategory.CategoryId = subcategoryData.CategoryId;

            await _context.SaveChangesAsync();
            return Success(subcategory, "Subcategory updated successfully.");
        }

        [HttpDelete("subcategories/{id}")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteSubcategory(int id)
        {
            var subcategory = await _context.Subcategories.FindAsync(id);
            if (subcategory == null)
            {
                return Error<int>("Subcategory not found.", 404);
            }

            _context.Subcategories.Remove(subcategory);
            await _context.SaveChangesAsync();

            return Success(id, "Subcategory deleted successfully.");
        }
    }
}
