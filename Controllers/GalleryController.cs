using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Data;
using BawaDittaMal.Api.Models;
using BawaDittaMal.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BawaDittaMal.Api.Controllers
{
    public class GalleryController : BaseApiController
    {
        private readonly AppDbContext _context;

        public GalleryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<GalleryItem>>>> GetGalleryItems()
        {
            var items = await _context.GalleryItems.ToListAsync();
            return Success(items);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GalleryItem>>> CreateGalleryItem([FromBody] GalleryItem item)
        {
            _context.GalleryItems.Add(item);
            await _context.SaveChangesAsync();
            return Success(item, "Gallery item added successfully.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<GalleryItem>>> UpdateGalleryItem(int id, [FromBody] GalleryItem itemData)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null)
            {
                return Error<GalleryItem>("Gallery item not found.", 404);
            }

            item.Title = itemData.Title;
            item.Category = itemData.Category;
            item.Image = itemData.Image;

            await _context.SaveChangesAsync();
            return Success(item, "Gallery item updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteGalleryItem(int id)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null)
            {
                return Error<int>("Gallery item not found.", 404);
            }

            _context.GalleryItems.Remove(item);
            await _context.SaveChangesAsync();
            return Success(id, "Gallery item deleted successfully.");
        }
    }
}
