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
    public class InquiriesController : BaseApiController
    {
        private readonly AppDbContext _context;

        public InquiriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedData<Inquiry>>>> GetInquiries(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            var query = _context.Inquiries.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var cleanSearch = search.Trim().ToLower();
                query = query.Where(i =>
                    i.Name.ToLower().Contains(cleanSearch) ||
                    i.Email.ToLower().Contains(cleanSearch) ||
                    i.Subject.ToLower().Contains(cleanSearch) ||
                    i.Message.ToLower().Contains(cleanSearch)
                );
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", System.StringComparison.OrdinalIgnoreCase))
            {
                var cleanStatus = status.Trim().ToLower();
                query = query.Where(i => i.Status.ToLower() == cleanStatus);
            }

            query = query.OrderByDescending(i => i.Id); // Newest first

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedData = new PaginatedData<Inquiry>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Success(paginatedData, "Inquiries retrieved successfully.");
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Inquiry>>> CreateInquiry([FromBody] Inquiry inquiry)
        {
            inquiry.Date = System.DateTime.Now.ToString("yyyy-MM-dd");
            inquiry.Status = "new";

            _context.Inquiries.Add(inquiry);
            await _context.SaveChangesAsync();

            return Success(inquiry, "Inquiry submitted successfully.");
        }

        [HttpPut("{id}/resolve")]
        public async Task<ActionResult<ApiResponse<Inquiry>>> ToggleResolveInquiry(int id)
        {
            var inquiry = await _context.Inquiries.FindAsync(id);
            if (inquiry == null)
            {
                return Error<Inquiry>("Inquiry not found.", 404);
            }

            inquiry.Status = inquiry.Status == "new" ? "resolved" : "new";
            await _context.SaveChangesAsync();

            return Success(inquiry, $"Inquiry marked as {inquiry.Status}.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<int>>> DeleteInquiry(int id)
        {
            var inquiry = await _context.Inquiries.FindAsync(id);
            if (inquiry == null)
            {
                return Error<int>("Inquiry not found.", 404);
            }

            _context.Inquiries.Remove(inquiry);
            await _context.SaveChangesAsync();

            return Success(id, "Inquiry deleted successfully.");
        }
    }
}
