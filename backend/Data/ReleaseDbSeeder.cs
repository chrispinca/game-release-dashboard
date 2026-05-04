using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class ReleaseDbSeeder
{
    public static async Task SeedAsync(ReleaseDbContext dbContext, CancellationToken cancellationToken)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Releases.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Releases.AddRange(
            new ReleaseRecord
            {
                Id = Guid.NewGuid(),
                GameName = "FIFA",
                TeamName = "Release Engineering",
                BuildVersion = "2026.05.01",
                Environment = "QA",
                Status = "Success",
                ReleaseNotes = "Validated the latest gameplay tuning package in QA.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new ReleaseRecord
            {
                Id = Guid.NewGuid(),
                GameName = "Madden",
                TeamName = "Backend Services",
                BuildVersion = "1.2.0",
                Environment = "Dev",
                Status = "Pending",
                ReleaseNotes = "Database migration queued before feature toggle rollout.",
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new ReleaseRecord
            {
                Id = Guid.NewGuid(),
                GameName = "Battlefield",
                TeamName = "Core Services",
                BuildVersion = "3.0.1",
                Environment = "Prod",
                Status = "Failed",
                ReleaseNotes = "Rollback triggered after matchmaking latency regression.",
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-6)
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
