using Microsoft.AspNetCore.Mvc;
using OhNoItsZombiesAnalyzer.Services;

namespace OhNoItsZombiesAnalyzer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReplayController : ControllerBase
    {
        private readonly ReplayService _replayService;

        public ReplayController(ReplayService replayService)
        {
            _replayService = replayService;
        }

        [HttpGet("list")]
        public IActionResult GetReplayList()
        {

            var replays = _replayService.GetReplayPathTuples();

            return Ok(replays);
        }

        [HttpGet("analyze")]
        public IActionResult AnalyzeReplay([FromQuery] string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return BadRequest("Path parameter is required");
            }

            var content = _replayService.GetReplayAnalyzeContent(path);
            return Ok(content);
        }
    }
}