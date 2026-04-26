using Microsoft.AspNetCore.Mvc;
using ONIZAnalyzer.Services;
using System.Net;

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

    [HttpGet("handle")]
    public IActionResult GetRecords()
    {
        var records = _recordsService.GetHandleDataFolder();
        return Ok(records);
    }

    [HttpGet("handle/{handle}")]
    public IActionResult GetCareerData(string handle)
    {
        var careerData = _recordsService.GetRecordTextData(handle);

        return Ok(careerData);
    }

    [HttpGet("profile-img/{handle}")]
    public async Task<IActionResult> GetProfileImgSrc(string handle)
    {
        var imgSrc = await _recordsService.GetProfileImageAsync(handle);

        return Ok(imgSrc);
    }
}