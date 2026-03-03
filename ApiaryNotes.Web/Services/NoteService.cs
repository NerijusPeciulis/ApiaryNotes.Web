using ApiaryNotes.Web.Data;
using ApiaryNotes.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Web.Application.Services;

public sealed class NoteService
{
    private readonly ApplicationDbContext db;

    public NoteService(ApplicationDbContext db) => this.db = db;

    public async Task<HiveNote> CreateAsync(
        string ownerUserId,
        int hiveId,
        DateOnly date,
        string? title,
        string text)
    {
        if (string.IsNullOrWhiteSpace(ownerUserId))
            throw new ArgumentException("Owner user id is required.", nameof(ownerUserId));

        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required.", nameof(text));

        var hive = await db.Hives.FirstOrDefaultAsync(h => h.Id == hiveId);
        if (hive is null)
            throw new KeyNotFoundException("Hive not found.");

        if (hive.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot add note to another user's hive.");

        var note = new HiveNote
        {
            OwnerUserId = ownerUserId,
            HiveId = hiveId,
            Date = date,
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            Text = text.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.HiveNotes.Add(note);
        await db.SaveChangesAsync();
        return note;
    }

    public Task<List<HiveNote>> GetForHiveAsync(string ownerUserId, int hiveId)
    {
        return db.HiveNotes
            .AsNoTracking()
            .Where(n => n.HiveId == hiveId && n.OwnerUserId == ownerUserId)
            .OrderByDescending(n => n.Date)
            .ThenByDescending(n => n.Id)
            .ToListAsync();
    }

    public async Task UpdateAsync(int noteId, string ownerUserId, DateOnly date, string? title, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is required.", nameof(text));

        var note = await db.HiveNotes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note is null)
            throw new KeyNotFoundException("Note not found.");

        if (note.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot modify another user's note.");

        note.Date = date;
        note.Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        note.Text = text.Trim();

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int noteId, string ownerUserId)
    {
        var note = await db.HiveNotes.FirstOrDefaultAsync(n => n.Id == noteId);
        if (note is null)
            throw new KeyNotFoundException("Note not found.");

        if (note.OwnerUserId != ownerUserId)
            throw new UnauthorizedAccessException("Cannot delete another user's note.");

        db.HiveNotes.Remove(note);
        await db.SaveChangesAsync();
    }
}