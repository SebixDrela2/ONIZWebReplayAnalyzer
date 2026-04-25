using Microsoft.AspNetCore.Mvc;
using ONIZAnalyzer.Services;

namespace ONIZAnalyzer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecordsController : ControllerBase
{
    private readonly RecordsService _recordsService;

    public RecordsController(RecordsService recordsService)
    {
        _recordsService = recordsService;
    }

    [HttpGet]
    [Route("handle")]
    public IActionResult GetRecords()
    {
        var records = _recordsService.GetHandleDataFolder();

        return Ok(records);
    }

    [HttpGet]
    [Route("handle/{handle}")]
    public IActionResult GetCareerData(string handle)
    {
        var careerData = _recordsService.GetSerializedCareerData(handle);

        return Ok(careerData);
    }
}
