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
    public class BrandsController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BrandsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Brand>>>> GetBrands()
        {
            var brands = await _context.Brands.ToListAsync();
            return Success(brands);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Brand>>> GetBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return Error<Brand>("Brand not found.", 404);
            }
            return Success(brand);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Brand>>> CreateBrand([FromBody] Brand brand)
        {
            brand.Url = FileHelper.SaveBase64Image(brand.Url, _env.ContentRootPath, "brand");
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return Success(brand, "Brand added successfully.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Brand>>> UpdateBrand(int id, [FromBody] Brand brandData)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return Error<Brand>("Brand not found.", 404);
            }

            brand.Name = brandData.Name;
            brand.Url = FileHelper.SaveBase64Image(brandData.Url, _env.ContentRootPath, "brand");
            brand.Link = brandData.Link;

            await _context.SaveChangesAsync();
            return Success(brand, "Brand updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteBrand(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return Error<int>("Brand not found.", 404);
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return Success(id, "Brand deleted successfully.");
        }
    }
}
