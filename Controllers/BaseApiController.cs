using Microsoft.AspNetCore.Mvc;
using BawaDittaMal.Api.DTOs;

namespace BawaDittaMal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        protected ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Success")
        {
            return Ok(ApiResponse<T>.SuccessResponse(data, message));
        }

        protected ActionResult<ApiResponse<T>> Error<T>(string message, int statusCode = 400)
        {
            var response = ApiResponse<T>.ErrorResponse(message);
            return StatusCode(statusCode, response);
        }
    }
}
