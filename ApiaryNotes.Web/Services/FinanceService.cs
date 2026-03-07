using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Application.Services;

public sealed class FinanceService
{
    private readonly ApplicationDbContext db;

    public FinanceService(ApplicationDbContext db)
    {
        this.db = db;
    }

    public async Task<FinanceEntry> CreateAsync(
        string ownerUserId,
        int? apiaryId,
        DateOnly date,
        FinanceEntryType type,
        FinanceCategory category,
        decimal amount,
        string title,
        string? note)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId))
            throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        if (apiaryId.HasValue)
        {
            var apiary = await db.Apiaries.FirstOrDefaultAsync(x => x.Id == apiaryId.Value);
            if (apiary is null)
                throw new KeyNotFoundException("Apiary not found.");

            if (apiary.OwnerUserId != ownerUserId)
                throw new UnauthorizedAccessException("Cannot add entry to another user's apiary.");
        }

        var entry = new FinanceEntry
        {
            OwnerUserId = ownerUserId,
            ApiaryId = apiaryId,
            Date = date,
            Type = type,
            Category = category,
            Amount = amount,
            Title = title.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        db.FinanceEntries.Add(entry);
        await db.SaveChangesAsync();

        return entry;
    }

    public Task<List<int>> GetYearsAsync(string ownerUserId)
    {
        return db.FinanceEntries
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId)
            .Select(x => x.Date.Year)
            .Distinct()
            .OrderByDescending(x => x)
            .ToListAsync();
    }

    public Task<List<FinanceEntry>> GetByYearAsync(string ownerUserId, int year)
    {
        return db.FinanceEntries
            .AsNoTracking()
            .Include(x => x.Apiary)
            .Where(x => x.OwnerUserId == ownerUserId && x.Date.Year == year)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<(decimal income, decimal expense, decimal profit)> GetSummaryByYearAsync(string ownerUserId, int year)
    {
        var list = await db.FinanceEntries
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.Date.Year == year)
            .ToListAsync();

        var income = list
            .Where(x => x.Type == FinanceEntryType.Income)
            .Sum(x => x.Amount);

        var expense = list
            .Where(x => x.Type == FinanceEntryType.Expense)
            .Sum(x => x.Amount);

        var profit = income - expense;

        return (income, expense, profit);
    }

    public async Task DeleteAsync(int id, string ownerUserId)
    {
        var entry = await db.FinanceEntries.FirstOrDefaultAsync(x => x.Id == id);
        if (entry is null)
            throw new KeyNotFoundException("Finance entry not found.");

        if (entry.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot delete another user's finance entry.");

        db.FinanceEntries.Remove(entry);
        await db.SaveChangesAsync();
    }
}