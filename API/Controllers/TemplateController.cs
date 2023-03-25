using Application.CQRS.Template;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class TemplateController: ApiV1Controller
{
    private readonly IMediator _mediator;
    public TemplateController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> AddTemplate(int Id)
    {
        return Ok(await _mediator.Send(new AddTemplateCommand(Id){}));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _mediator.Send(new GetAllTemplateQuery()));
    }
}