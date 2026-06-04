using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Data;
using BawaDittaMal.Api.Models;
using BawaDittaMal.Api.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BawaDittaMal.Api.Controllers
{
    public class TestimonialsController : BaseApiController
    {
        private readonly AppDbContext _context;

        public TestimonialsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Testimonial>>>> GetTestimonials()
        {
            var testimonials = await _context.Testimonials.ToListAsync();
            return Success(testimonials);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Testimonial>>> CreateTestimonial([FromBody] Testimonial testimonial)
        {
            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();
            return Success(testimonial, "Testimonial created successfully.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Testimonial>>> UpdateTestimonial(int id, [FromBody] Testimonial testimonialData)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return Error<Testimonial>("Testimonial not found.", 404);
            }

            testimonial.Name = testimonialData.Name;
            testimonial.Role = testimonialData.Role;
            testimonial.Company = testimonialData.Company;
            testimonial.Category = testimonialData.Category;
            testimonial.Quote = testimonialData.Quote;
            testimonial.Rating = testimonialData.Rating;
            testimonial.Status = testimonialData.Status;

            await _context.SaveChangesAsync();
            return Success(testimonial, "Testimonial updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteTestimonial(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return Error<int>("Testimonial not found.", 404);
            }

            _context.Testimonials.Remove(testimonial);
            await _context.SaveChangesAsync();
            return Success(id, "Testimonial deleted successfully.");
        }
    }
}
