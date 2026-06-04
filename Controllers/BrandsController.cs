using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Data;
using BawaDittaMal.Api.Models;
using BawaDittaMal.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BawaDittaMal.Api.Controllers
{
    public class BrandsController : BaseApiController
    {
        private readonly AppDbContext _context;

        public BrandsController(AppDbContext context)
        {
            _context = context;
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
            brand.Url = brandData.Url;
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
