using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Application.Services;

public sealed class ApiaryService
{
    private readonly ApplicationDbContext db;

    public ApiaryService(ApplicationDbContext db) => this.db = db;

    public async Task<Apiary> CreateAsync(string ownerUserId, string name, string? location)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId))
            throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        var apiary = new Apiary
        {
            OwnerUserId = ownerUserId,
            Name = name.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.Apiaries.Add(apiary);
        await db.SaveChangesAsync();
        return apiary;
    }

    public Task<List<Apiary>> GetForUserAsync(string ownerUserId)
    {
        return db.Apiaries
            .AsNoTracking()
            .Where(a => a.OwnerUserId == ownerUserId)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task UpdateAsync(int apiaryId, string ownerUserId, string name, string? location)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        var apiary = await db.Apiaries.FirstOrDefaultAsync(a => a.Id == apiaryId);
        if (apiary is null)
            throw new KeyNotFoundException("Apiary not found.");

        if (apiary.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot modify another user's apiary.");

        apiary.Name = name.Trim();
        apiary.Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int apiaryId, string ownerUserId)
    {
        var apiary = await db.Apiaries.FirstOrDefaultAsync(a => a.Id == apiaryId);
        if (apiary is null)
            throw new KeyNotFoundException("Apiary not found.");

        if (apiary.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot delete another user's apiary.");

        db.Apiaries.Remove(apiary);
        await db.SaveChangesAsync();
    }
}