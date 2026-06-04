using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Data;
using BawaDittaMal.Api.Models;
using BawaDittaMal.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BawaDittaMal.Api.Controllers
{
    public class SliderController : BaseApiController
    {
        private readonly AppDbContext _context;

        public SliderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SliderItem>>>> GetSliderItems()
        {
            var items = await _context.SliderItems.ToListAsync();
            return Success(items);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<SliderItem>>> CreateSliderItem([FromBody] SliderItem item)
        {
            _context.SliderItems.Add(item);
            await _context.SaveChangesAsync();
            return Success(item, "Slider item added successfully.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<SliderItem>>> UpdateSliderItem(int id, [FromBody] SliderItem itemData)
        {
            var item = await _context.SliderItems.FindAsync(id);
            if (item == null)
            {
                return Error<SliderItem>("Slider item not found.", 404);
            }

            item.Image = itemData.Image;
            item.Title = itemData.Title;
            item.Subtitle = itemData.Subtitle;
            item.Description = itemData.Description;
            item.BtnText = itemData.BtnText;
            item.BtnLink = itemData.BtnLink;
            item.Btn1Text = itemData.Btn1Text;
            item.Btn1Link = itemData.Btn1Link;
            item.Btn2Text = itemData.Btn2Text;
            item.Btn2Link = itemData.Btn2Link;
            item.Status = itemData.Status;

            await _context.SaveChangesAsync();
            return Success(item, "Slider item updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteSliderItem(int id)
        {
            var item = await _context.SliderItems.FindAsync(id);
            if (item == null)
            {
                return Error<int>("Slider item not found.", 404);
            }

            _context.SliderItems.Remove(item);
            await _context.SaveChangesAsync();
            return Success(id, "Slider item deleted successfully.");
        }
    }
}
