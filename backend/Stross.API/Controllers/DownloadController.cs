using Stross.Infrastructure.Services.GrpcService;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Stross.API.Controllers;

/// <summary>
/// Controller for handling download operations
/// </summary>
public class DownloadController: ApiV1Controller
{
    private readonly IGrpcService _grpcService;

    public DownloadController(IGrpcService grpcService)
    {
        _grpcService = grpcService;
    }

    /// <summary>
    /// Downloads YouTube audio from the provided URL
    /// </summary>
    /// <param name="url">The YouTube URL to download audio from</param>
    /// <returns>The result of the download operation</returns>
    /// <response code="200">Successfully initiated the download</response>
    /// <response code="400">Invalid URL provided</response>
    [HttpPost("youtube-audio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddTemplate([FromBody] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest("URL cannot be empty");
        }

        object result = await _grpcService.SendDownloadYtAudioAsync(url);

        return Ok(result);
    }
}