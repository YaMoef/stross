using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class TemplateConfiguration: IEntityTypeConfiguration<TemplateModel>
{
    public void Configure(EntityTypeBuilder<TemplateModel> builder)
    {
        builder.HasIndex(b => b.Id);
    }
}