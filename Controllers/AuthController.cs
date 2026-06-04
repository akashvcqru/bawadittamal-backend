using Microsoft.AspNetCore.Mvc;
using BawaDittaMal.Api.DTOs;

namespace BawaDittaMal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        [HttpPost("login")]
        public ActionResult<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (request.Email == "admin@bawadittamal.com" && request.Password == "admin123")
            {
                var response = new LoginResponse
                {
                    Email = request.Email,
                    Name = "Bawa Ditta Mal",
                    Role = "admin",
                    Token = "dummy-jwt-token-value-for-bdm-admin"
                };
                return Success(response, "Welcome back! Login successful.");
            }

            return Error<LoginResponse>("Invalid email or password.", 401);
        }
    }
}
