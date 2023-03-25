using System.Reflection;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public class TemplateContext: DbContext
{
    public TemplateContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TemplateModel> TemplateModels {get; set; } = null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<TemplateModel>();

    }
}

