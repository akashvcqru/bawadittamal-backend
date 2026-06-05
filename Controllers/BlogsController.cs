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
    public class BlogsController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BlogsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Blog>>>> GetBlogs()
        {
            var blogs = await _context.Blogs.ToListAsync();
            return Success(blogs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Blog>>> GetBlog(string id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                blog = await _context.Blogs.FirstOrDefaultAsync(b => b.Slug == id);
                if (blog == null)
                {
                    return Error<Blog>("Blog article not found.", 404);
                }
            }
            return Success(blog);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Blog>>> CreateBlog([FromBody] Blog blog)
        {
            if (string.IsNullOrWhiteSpace(blog.Id))
            {
                blog.Id = "BLOG-" + System.Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            }

            if (string.IsNullOrWhiteSpace(blog.Slug))
            {
                blog.Slug = blog.Title.ToLower().Replace(" ", "-").Replace("?", "");
            }

            if (string.IsNullOrWhiteSpace(blog.Date))
            {
                blog.Date = System.DateTime.Now.ToString("MMM dd, yyyy");
            }

            blog.Image = FileHelper.SaveBase64Image(blog.Image, _env.ContentRootPath, "blog");

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();
            return Success(blog, "Blog post published successfully.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Blog>>> UpdateBlog(string id, [FromBody] Blog blogData)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return Error<Blog>("Blog post not found.", 404);
            }

            blog.Title = blogData.Title;
            blog.Slug = blogData.Slug;
            blog.Image = FileHelper.SaveBase64Image(blogData.Image, _env.ContentRootPath, "blog");
            blog.Category = blogData.Category;
            blog.AuthorName = blogData.AuthorName;
            blog.AuthorRole = blogData.AuthorRole;
            blog.Tags = blogData.Tags;
            blog.Excerpt = blogData.Excerpt;
            blog.Content = blogData.Content;
            blog.ReadTime = blogData.ReadTime;

            await _context.SaveChangesAsync();
            return Success(blog, "Blog post updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteBlog(string id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return Error<string>("Blog post not found.", 404);
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();
            return Success(id, "Blog post deleted successfully.");
        }
    }
}
