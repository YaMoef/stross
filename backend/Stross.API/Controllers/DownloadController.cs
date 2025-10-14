using Stross.Infrastructure.Services.GrpcService;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Stross.API.Controllers;

public class DownloadController: ApiV1Controller
{
    private readonly IMediator _mediator;
    private readonly IGrpcService _grpcService;

    public DownloadController(IMediator mediator, IGrpcService grpcService)
    {
        _mediator = mediator;
        _grpcService = grpcService;
    }

    [HttpPost]
    public async Task<IActionResult> AddTemplate([FromBody] string url)
    {
        var result = await _grpcService.SendDownloadYtAudio(url);

        return Ok(result);
    }
}