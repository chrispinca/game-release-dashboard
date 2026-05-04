using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public sealed class ReleaseRepository(ReleaseDbContext dbContext) : IReleaseRepository
{
    public async Task<IReadOnlyList<ReleaseRecord>> GetAllAsync(ReleaseQueryParameters query, CancellationToken cancellationToken)
    {
        var releaseQuery = dbContext.Releases.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Environment))
        {
            releaseQuery = releaseQuery.Where(release =>
                release.Environment.ToLower() == query.Environment.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            releaseQuery = releaseQuery.Where(release =>
                release.Status.ToLower() == query.Status.ToLower());
        }

        var releases = await releaseQuery.ToListAsync(cancellationToken);
        return releases
            .OrderByDescending(release => release.CreatedAt)
            .ToList();
    }

    public async Task<ReleaseRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Releases
            .AsNoTracking()
            .FirstOrDefaultAsync(release => release.Id == id, cancellationToken);
    }

    public async Task<ReleaseRecord> CreateAsync(ReleaseUpsertRequest request, CancellationToken cancellationToken)
    {
        var newRelease = new ReleaseRecord
        {
            Id = Guid.NewGuid(),
            GameName = request.GameName.Trim(),
            TeamName = request.TeamName.Trim(),
            BuildVersion = request.BuildVersion.Trim(),
            Environment = request.Environment.Trim(),
            Status = request.Status.Trim(),
            ReleaseNotes = request.ReleaseNotes.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Releases.Add(newRelease);
        await dbContext.SaveChangesAsync(cancellationToken);

        return newRelease;
    }

    public async Task<ReleaseRecord?> UpdateAsync(Guid id, ReleaseUpsertRequest request, CancellationToken cancellationToken)
    {
        var existingRelease = await dbContext.Releases.FirstOrDefaultAsync(
            release => release.Id == id,
            cancellationToken);

        if (existingRelease is null)
        {
            return null;
        }

        existingRelease.GameName = request.GameName.Trim();
        existingRelease.TeamName = request.TeamName.Trim();
        existingRelease.BuildVersion = request.BuildVersion.Trim();
        existingRelease.Environment = request.Environment.Trim();
        existingRelease.Status = request.Status.Trim();
        existingRelease.ReleaseNotes = request.ReleaseNotes.Trim();

        await dbContext.SaveChangesAsync(cancellationToken);
        return existingRelease;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existingRelease = await dbContext.Releases.FirstOrDefaultAsync(
            release => release.Id == id,
            cancellationToken);

        if (existingRelease is null)
        {
            return false;
        }

        dbContext.Releases.Remove(existingRelease);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
