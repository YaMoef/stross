using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class UserApiKeyConfiguration : BaseEntityConfiguration<UserApiKey>
{
    public readonly int KeyNameMaxLength = 255;

    protected override void ConfigureEntity(EntityTypeBuilder<UserApiKey> builder)
    {
        builder.ToTable("UserApiKeys");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property<string>("_apiKey")
            .HasField("_apiKey")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.KeyName)
            .IsRequired()
            .HasMaxLength(KeyNameMaxLength);
    }
}