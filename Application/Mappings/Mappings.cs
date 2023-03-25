using Application.CQRS.Template;
using AutoMapper;
using Domain;

namespace Application.Mappings;

public class Mappings : Profile
{
    public Mappings()
    {
        CreateMap<TemplateModel, TemplateDto>();
    }
}