using Domain;
using Infrastructure.Contexts;
using MediatR;

namespace Application.CQRS.Template;

public class AddTemplateCommand : IRequest<Unit>
{
    public int Id { get;}

    public AddTemplateCommand(int userId)
    {
        Id = userId;
    }
}

public class AddTemplateCommandHandler : IRequestHandler<AddTemplateCommand, Unit>
{
    private readonly TemplateContext _ctx;

    public AddTemplateCommandHandler(TemplateContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Unit> Handle(AddTemplateCommand request, CancellationToken cancellationToken)
    {
        await _ctx.TemplateModels.AddAsync(new TemplateModel(request.Id), cancellationToken);

        await _ctx.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}