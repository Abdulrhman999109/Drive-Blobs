using Drive_project.Config;
using Drive_project.Services.IServices;
using Drive_project.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Drive_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetTokenController : ControllerBase
    {
        private readonly IConfiguration _config;
        public GetTokenController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("token")]
        public IActionResult GetToken()
        {
            var token = JwtUtil.IssueJwt(_config, new { id = "client", issued_at = DateTime.UtcNow }, expiresInSeconds: 3600);
            return Ok(new { token });
        }
    }
}
