using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReleasesController(IReleaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReleaseRecord>>> GetAll(
        [FromQuery] ReleaseQueryParameters query,
        CancellationToken cancellationToken)
    {
        var releases = await repository.GetAllAsync(query, cancellationToken);
        return Ok(releases);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReleaseRecord>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var release = await repository.GetByIdAsync(id, cancellationToken);
        return release is null ? NotFound() : Ok(release);
    }

    [HttpPost]
    public async Task<ActionResult<ReleaseRecord>> Create(
        [FromBody] ReleaseUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var createdRelease = await repository.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdRelease.Id }, createdRelease);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReleaseRecord>> Update(
        Guid id,
        [FromBody] ReleaseUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var updatedRelease = await repository.UpdateAsync(id, request, cancellationToken);
        return updatedRelease is null ? NotFound() : Ok(updatedRelease);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
