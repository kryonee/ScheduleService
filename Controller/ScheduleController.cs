using Microsoft.AspNetCore.Mvc;
using Schedule.Model;
using Schedule.Services;
using System.Runtime.InteropServices;

namespace Schedule.Api.Controllers
{
    [ApiController]
    [Route("api/Public")]
    [Produces("application/json")]
    public class PublicController : ControllerBase
    {
        private readonly SchedulerService _schedulerService;
        private readonly ILogger<PublicController> _logger;
        private readonly IConfiguration _configuration;

        public PublicController(SchedulerService schedulerService, ILogger<PublicController> logger, IConfiguration configuration)
        {
            _schedulerService = schedulerService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] TimeTableRequest input)
        {
            var validApiKey = _configuration["ApiSettings:ApiKey"];
            if (!Request.Headers.TryGetValue("Api-Key", out var apiKey) || apiKey != validApiKey)
                return Unauthorized(new { Message = "Missing or invalid Api-Key" });

            if (input == null)
                return BadRequest(new { Message = "Request body cannot be null" });

            var result = await Task.Run(() => _schedulerService.Generate(input));
            return Ok(new { Success = true, Data = result });
        }
    }
}
