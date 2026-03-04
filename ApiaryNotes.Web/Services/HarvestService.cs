using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Application.Services;

public sealed class HarvestService
{
    private readonly ApplicationDbContext db;

    public HarvestService(ApplicationDbContext db) => this.db = db;

    public async Task<HiveHarvest> CreateAsync(
        string ownerUserId,
        int hiveId,
        DateOnly date,
        decimal amount,
        HarvestUnit unit,
        string? note)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId))
            throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");

        var hive = await db.Hives.FirstOrDefaultAsync(h => h.Id == hiveId);
        if (hive is null)
            throw new KeyNotFoundException("Hive not found.");

        if (hive.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot add harvest to another user's hive.");

        var harvest = new HiveHarvest
        {
            OwnerUserId = ownerUserId,
            HiveId = hiveId,
            Date = date,
            Amount = amount,
            Unit = unit,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.HiveHarvests.Add(harvest);
        await db.SaveChangesAsync();
        return harvest;
    }

    public Task<List<HiveHarvest>> GetForHiveAsync(string ownerUserId, int hiveId)
    {
        return db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.HiveId == hiveId && x.OwnerUserId == ownerUserId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task DeleteAsync(int harvestId, string ownerUserId)
    {
        var harvest = await db.HiveHarvests.FirstOrDefaultAsync(x => x.Id == harvestId);
        if (harvest is null)
            throw new KeyNotFoundException("Harvest not found.");

        if (harvest.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot delete another user's harvest.");

        db.HiveHarvests.Remove(harvest);
        await db.SaveChangesAsync();
    }

    public async Task<(decimal kg, decimal l)> GetTotalsForHiveAsync(string ownerUserId, int hiveId)
    {
        var list = await db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.HiveId == hiveId && x.OwnerUserId == ownerUserId)
            .ToListAsync();

        var totalKg = list.Where(x => x.Unit == HarvestUnit.Kg).Sum(x => x.Amount);
        var totalL = list.Where(x => x.Unit == HarvestUnit.L).Sum(x => x.Amount);

        return (totalKg, totalL);
    }
}