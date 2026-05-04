using backend.Models;

namespace backend.Repositories;

public interface IReleaseRepository
{
    Task<IReadOnlyList<ReleaseRecord>> GetAllAsync(ReleaseQueryParameters query, CancellationToken cancellationToken);
    Task<ReleaseRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ReleaseRecord> CreateAsync(ReleaseUpsertRequest request, CancellationToken cancellationToken);
    Task<ReleaseRecord?> UpdateAsync(Guid id, ReleaseUpsertRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
