using Drive_project.Dto;
using Drive_project.Services.IServices;
using Drive_project.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Drive_project.Controllers
{
    [Authorize]
    [Route("v1/[controller]")]
    [ApiController]
    public class BlobsController : ControllerBase
    {
        private readonly IBlobsService _service;
        private readonly IConfiguration _config;

        public BlobsController(IBlobsService service , IConfiguration config)
        {
            _service = service;
            _config = config;

        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBlobDto dto)
        {
            try
            {
                var result = await _service.CreateBlobAsync(dto.Id, dto.Data, dto.Backend ?? "db");
                return StatusCode(201, new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var result = await _service.GetAsync(id);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
