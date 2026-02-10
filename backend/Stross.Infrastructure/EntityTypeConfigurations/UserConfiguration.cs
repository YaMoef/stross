using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stross.Domain.Entities;

namespace Stross.Infrastructure.EntityTypeConfigurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public static readonly int UserNameMaxLength = 255;
    public static readonly int DisplayNameMaxLength = 255;
    public static readonly int PasswordMaxLength = 255;

    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(UserNameMaxLength);

        builder.HasIndex(x => x.UserName)
            .IsUnique();

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(DisplayNameMaxLength);

        builder.Property(x => x.Password)
            .HasMaxLength(PasswordMaxLength);

        builder.Property(x => x.IsDefaultUser)
            .IsRequired();

        builder.HasMany(x => x.UserApiKeys);
    }
}
