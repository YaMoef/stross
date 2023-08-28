using AutoMapper;
using Domain;
using Infrastructure.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CQRS.Template;

public class GetAllTemplateQuery : IRequest<IEnumerable<TemplateDto>>
{
    public GetAllTemplateQuery()
    {
        
    }
}

public class GetAllTemplateQueryHandler : IRequestHandler<GetAllTemplateQuery, IEnumerable<TemplateDto>>
{
    private readonly TemplateContext _ctx;
    private readonly IMapper _mapper;
    
    public GetAllTemplateQueryHandler(TemplateContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }


    public async Task<IEnumerable<TemplateDto>> Handle(GetAllTemplateQuery request, CancellationToken cancellationToken)
    {
        List<TemplateModel> templates = await _ctx.TemplateModels.ToListAsync();
        return _mapper.Map<IEnumerable<TemplateDto>>(templates);
    }
}