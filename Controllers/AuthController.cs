using Microsoft.AspNetCore.Mvc;

namespace MiVivienda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string UserName);

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        // Credenciales DEMO (las que usaremos en el front)
        if (request.Username == "superadmin@demo.com" && request.Password == "demo123")
        {
            return Ok(new LoginResponse(
                Token: "demo-token-superadmin",
                UserName: "Super Admin"
            ));
        }

        return Unauthorized(new { message = "Credenciales inválidas" });
    }
}