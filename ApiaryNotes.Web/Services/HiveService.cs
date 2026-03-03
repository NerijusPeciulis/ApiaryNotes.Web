using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Application.Services;

public sealed class HiveService
{
    private readonly ApplicationDbContext db;

    public HiveService(ApplicationDbContext db) => this.db = db;

    public async Task<Hive> CreateAsync(string ownerUserId, int apiaryId, string code, string? hiveType)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId))
            throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Hive code is required.", nameof(code));

        var apiary = await db.Apiaries.FirstOrDefaultAsync(a => a.Id == apiaryId);
        if (apiary is null)
            throw new KeyNotFoundException("Apiary not found.");

        if (apiary.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot add hive to another user's apiary.");

        var normalizedCode = code.Trim();

        var exists = await db.Hives.AnyAsync(h =>
            h.ApiaryId == apiaryId &&
            h.OwnerUserId == ownerUserId &&
            h.Code.ToLower() == normalizedCode.ToLower());

        if (exists)
            throw new InvalidOperationException("Toks avilio pavadinimas jau egzistuoja šiame bityne.");



        var hive = new Hive
        {
            OwnerUserId = ownerUserId,
            ApiaryId = apiaryId,
            Code = normalizedCode,
            HiveType = string.IsNullOrWhiteSpace(hiveType) ? null : hiveType.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.Hives.Add(hive);
        await db.SaveChangesAsync();
        return hive;
    }

    public Task<List<Hive>> GetForApiaryAsync(string ownerUserId, int apiaryId)
    {
        return db.Hives
            .AsNoTracking()
            .Where(h => h.ApiaryId == apiaryId && h.OwnerUserId == ownerUserId)
            .OrderBy(h => h.Code)
            .ToListAsync();
    }

    public async Task UpdateAsync(int hiveId, string ownerUserId, string code, string? hiveType)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Hive code is required.", nameof(code));

        var hive = await db.Hives.FirstOrDefaultAsync(h => h.Id == hiveId);
        if (hive is null)
            throw new KeyNotFoundException("Hive not found.");

        if (hive.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot modify another user's hive.");

        var normalizedCode = code.Trim();

        var exists = await db.Hives.AnyAsync(h =>
            h.Id != hiveId &&
            h.ApiaryId == hive.ApiaryId &&
            h.OwnerUserId == ownerUserId &&
            h.Code.ToLower() == normalizedCode.ToLower());

        if (exists)
            throw new InvalidOperationException("Toks avilio pavadinimas jau egzistuoja šiame bityne.");

        hive.Code = normalizedCode;

        hive.HiveType = string.IsNullOrWhiteSpace(hiveType) ? null : hiveType.Trim();

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int hiveId, string ownerUserId)
    {
        var hive = await db.Hives.FirstOrDefaultAsync(h => h.Id == hiveId);
        if (hive is null)
            throw new KeyNotFoundException("Hive not found.");

        if (hive.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot delete another user's hive.");

        db.Hives.Remove(hive);
        await db.SaveChangesAsync();
    }
}