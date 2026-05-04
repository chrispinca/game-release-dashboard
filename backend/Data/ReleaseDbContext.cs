using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public sealed class ReleaseDbContext(DbContextOptions<ReleaseDbContext> options) : DbContext(options)
{
    public DbSet<ReleaseRecord> Releases => Set<ReleaseRecord>();
}
