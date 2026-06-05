using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Data;
using BawaDittaMal.Api.Models;
using BawaDittaMal.Api.DTOs;
using BawaDittaMal.Api.Helpers;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BawaDittaMal.Api.Controllers
{
    public class GalleryController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public GalleryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
            item.Image = FileHelper.SaveBase64Image(item.Image, _env.ContentRootPath, "gallery");
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
            item.Image = FileHelper.SaveBase64Image(itemData.Image, _env.ContentRootPath, "gallery");

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
