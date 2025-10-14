using Microsoft.EntityFrameworkCore;

namespace Stross.Infrastructure;

public class StrossContext : DbContext
{
    public StrossContext(DbContextOptions<StrossContext> options) : base(options)
    {
        
    }
}