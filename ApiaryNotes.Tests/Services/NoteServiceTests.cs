using ApiaryNotes.Tests.Helpers;
using ApiaryNotes.Web.Application.Services;
using NUnit.Framework;

namespace ApiaryNotes.Tests.Services;

[TestFixture]
public class NoteServiceTests
{
    [Test]
    public async Task CreateAsync_WhenHiveNotFound_ThrowsKeyNotFound()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_WhenHiveNotFound_ThrowsKeyNotFound));
        var sut = new NoteService(db);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await sut.CreateAsync("user-1", hiveId: 999, DateOnly.FromDateTime(DateTime.UtcNow), "T", "Text"));
    }

    [Test]
    public async Task CreateAsync_WhenHiveBelongsToAnotherUser_ThrowsUnauthorized()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_WhenHiveBelongsToAnotherUser_ThrowsUnauthorized));
        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);
        var sut = new NoteService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);
        var hive = await hiveService.CreateAsync("user-1", apiary.Id, "A1", null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await sut.CreateAsync("user-2", hive.Id, DateOnly.FromDateTime(DateTime.UtcNow), null, "Bandau"));
    }

    [Test]
    public async Task CreateAsync_WhenTextEmpty_ThrowsArgumentException()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_WhenTextEmpty_ThrowsArgumentException));
        var sut = new NoteService(db);

        Assert.ThrowsAsync<ArgumentException>(async () =>
            await sut.CreateAsync("user-1", hiveId: 1, DateOnly.FromDateTime(DateTime.UtcNow), null, "   "));
    }

    [Test]
    public async Task CreateAsync_ValidInput_SavesNote()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_ValidInput_SavesNote));
        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);
        var sut = new NoteService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);
        var hive = await hiveService.CreateAsync("user-1", apiary.Id, "A1", null);

        var date = new DateOnly(2026, 3, 3);
        var note = await sut.CreateAsync("user-1", hive.Id, date, "Patikra", "Viskas ok");

        Assert.That(note.Id, Is.GreaterThan(0));
        Assert.That(note.OwnerUserId, Is.EqualTo("user-1"));
        Assert.That(note.HiveId, Is.EqualTo(hive.Id));
        Assert.That(note.Date, Is.EqualTo(date));
        Assert.That(note.Title, Is.EqualTo("Patikra"));
        Assert.That(note.Text, Is.EqualTo("Viskas ok"));
    }

    [Test]
    public async Task GetForHiveAsync_ReturnsOnlyOwnersNotes()
    {
        using var db = DbContextFactory.Create(nameof(GetForHiveAsync_ReturnsOnlyOwnersNotes));
        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);
        var sut = new NoteService(db);

        var apiary1 = await apiaryService.CreateAsync("user-1", "B1", null);
        var hive1 = await hiveService.CreateAsync("user-1", apiary1.Id, "A1", null);

        var apiary2 = await apiaryService.CreateAsync("user-2", "B2", null);
        var hive2 = await hiveService.CreateAsync("user-2", apiary2.Id, "B1", null);

        await sut.CreateAsync("user-1", hive1.Id, new DateOnly(2026, 3, 1), null, "U1-1");
        await sut.CreateAsync("user-1", hive1.Id, new DateOnly(2026, 3, 2), null, "U1-2");
        await sut.CreateAsync("user-2", hive2.Id, new DateOnly(2026, 3, 3), null, "U2-1");

        var list = await sut.GetForHiveAsync("user-1", hive1.Id);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.All(n => n.OwnerUserId == "user-1"), Is.True);
        Assert.That(list.All(n => n.HiveId == hive1.Id), Is.True);
    }

    [Test]
    public async Task UpdateAsync_WhenNotOwner_ThrowsUnauthorized()
    {
        using var db = DbContextFactory.Create(nameof(UpdateAsync_WhenNotOwner_ThrowsUnauthorized));
        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);
        var sut = new NoteService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);
        var hive = await hiveService.CreateAsync("user-1", apiary.Id, "A1", null);
        var note = await sut.CreateAsync("user-1", hive.Id, new DateOnly(2026, 3, 3), null, "Text");

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await sut.UpdateAsync(note.Id, "user-2", new DateOnly(2026, 3, 4), "H", "Hack"));
    }

    [Test]
    public async Task DeleteAsync_WhenNotOwner_ThrowsUnauthorized()
    {
        using var db = DbContextFactory.Create(nameof(DeleteAsync_WhenNotOwner_ThrowsUnauthorized));
        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);
        var sut = new NoteService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);
        var hive = await hiveService.CreateAsync("user-1", apiary.Id, "A1", null);
        var note = await sut.CreateAsync("user-1", hive.Id, new DateOnly(2026, 3, 3), null, "Text");

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await sut.DeleteAsync(note.Id, "user-2"));
    }
}