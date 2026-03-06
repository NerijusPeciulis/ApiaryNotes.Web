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

    public Task<List<HiveHarvest>> GetForHiveByYearAsync(string ownerUserId, int hiveId, int year)
    {
        return db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.HiveId == hiveId
                && x.OwnerUserId == ownerUserId
                && x.Date.Year == year)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public Task<List<int>> GetYearsForHiveAsync(string ownerUserId, int hiveId)
    {
        return db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.HiveId == hiveId && x.OwnerUserId == ownerUserId)
            .Select(x => x.Date.Year)
            .Distinct()
            .OrderByDescending(x => x)
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

    public async Task<(decimal kg, decimal l)> GetTotalsForHiveByYearAsync(string ownerUserId, int hiveId, int year)
    {
        var list = await db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.HiveId == hiveId
                && x.OwnerUserId == ownerUserId
                && x.Date.Year == year)
            .ToListAsync();

        var totalKg = list.Where(x => x.Unit == HarvestUnit.Kg).Sum(x => x.Amount);
        var totalL = list.Where(x => x.Unit == HarvestUnit.L).Sum(x => x.Amount);

        return (totalKg, totalL);
    }

    public Task<List<int>> GetYearsForApiaryAsync(string ownerUserId, int apiaryId)
    {
        return db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.Hive.ApiaryId == apiaryId)
            .Select(x => x.Date.Year)
            .Distinct()
            .OrderByDescending(x => x)
            .ToListAsync();
    }

    public async Task<(decimal kg, decimal l)> GetTotalsForApiaryByYearAsync(string ownerUserId, int apiaryId, int year)
    {
        var list = await db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId
                && x.Hive.ApiaryId == apiaryId
                && x.Date.Year == year)
            .ToListAsync();

        var totalKg = list.Where(x => x.Unit == HarvestUnit.Kg).Sum(x => x.Amount);
        var totalL = list.Where(x => x.Unit == HarvestUnit.L).Sum(x => x.Amount);

        return (totalKg, totalL);
    }

    public Task<List<ApiaryHarvestStatRow>> GetApiaryStatsByYearAsync(string ownerUserId, int apiaryId, int year)
    {
        return db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId
                && x.Hive.ApiaryId == apiaryId
                && x.Date.Year == year)
            .GroupBy(x => new { x.HiveId, x.Hive.Code })
            .Select(g => new ApiaryHarvestStatRow
            {
                HiveId = g.Key.HiveId,
                HiveCode = g.Key.Code,
                TotalKg = g.Where(x => x.Unit == HarvestUnit.Kg).Sum(x => x.Amount),
                TotalL = g.Where(x => x.Unit == HarvestUnit.L).Sum(x => x.Amount),
            })
            .OrderByDescending(x => x.TotalKg)
            .ThenBy(x => x.HiveCode)
            .ToListAsync();
    }

    public async Task<List<MonthlyHarvestPoint>> GetMonthlyTotalsForApiaryAsync(string ownerUserId, int apiaryId, int year)
    {
        var raw = await db.HiveHarvests
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId
                && x.Hive.ApiaryId == apiaryId
                && x.Date.Year == year)
            .GroupBy(x => x.Date.Month)
            .Select(g => new MonthlyHarvestPoint
            {
                Month = g.Key,
                TotalKg = g.Where(x => x.Unit == HarvestUnit.Kg).Sum(x => x.Amount),
                TotalL = g.Where(x => x.Unit == HarvestUnit.L).Sum(x => x.Amount),
            })
            .ToListAsync();

        var result = Enumerable.Range(1, 12)
            .Select(month => raw.FirstOrDefault(x => x.Month == month) ?? new MonthlyHarvestPoint
            {
                Month = month,
                TotalKg = 0,
                TotalL = 0,
            })
            .OrderBy(x => x.Month)
            .ToList();

        return result;
    }
}