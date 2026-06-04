using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Data;
using BawaDittaMal.Api.Models;
using BawaDittaMal.Api.DTOs;
using System.Threading.Tasks;
using System.Text.Json;

namespace BawaDittaMal.Api.Controllers
{
    public class SettingsController : BaseApiController
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{key}")]
        public async Task<ActionResult<ApiResponse<object>>> GetSetting(string key)
        {
            var setting = await _context.Settings.FindAsync(key);
            if (setting == null)
            {
                return Error<object>($"Setting with key '{key}' not found.", 404);
            }

            try
            {
                // Deserialize to a generic object to return clean JSON
                var parsedValue = JsonSerializer.Deserialize<object>(setting.Value) ?? new object();
                return Success(parsedValue);
            }
            catch
            {
                return Success<object>(setting.Value); // Fallback to raw string
            }
        }

        [HttpPost("{key}")]
        public async Task<ActionResult<ApiResponse<string>>> SaveSetting(string key, [FromBody] JsonElement jsonValue)
        {
            var setting = await _context.Settings.FindAsync(key);
            var serializedValue = jsonValue.GetRawText();

            if (setting == null)
            {
                setting = new Setting { Key = key, Value = serializedValue };
                _context.Settings.Add(setting);
            }
            else
            {
                setting.Value = serializedValue;
            }

            await _context.SaveChangesAsync();
            return Success(key, $"Setting '{key}' saved successfully.");
        }
    }
}
