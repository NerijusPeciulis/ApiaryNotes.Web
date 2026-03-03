using ApiaryNotes.Tests.Helpers;
using ApiaryNotes.Web.Application.Services;
using NUnit.Framework;

namespace ApiaryNotes.Tests.Services;

[TestFixture]
public class HiveServiceTests
{
    [Test]
    public async Task CreateAsync_WhenApiaryNotFound_ThrowsKeyNotFound()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_WhenApiaryNotFound_ThrowsKeyNotFound));
        var sut = new HiveService(db);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await sut.CreateAsync("user-1", apiaryId: 999, code: "A1", hiveType: null));
    }

    [Test]
    public async Task CreateAsync_WhenApiaryBelongsToAnotherUser_ThrowsUnauthorized()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_WhenApiaryBelongsToAnotherUser_ThrowsUnauthorized));
        var apiaryService = new ApiaryService(db);
        var sut = new HiveService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await sut.CreateAsync("user-2", apiary.Id, "A1", null));
    }

    [Test]
    public async Task CreateAsync_ValidInput_SavesHiveWithOwnerAndApiaryId()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_ValidInput_SavesHiveWithOwnerAndApiaryId));
        var apiaryService = new ApiaryService(db);
        var sut = new HiveService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);
        var hive = await sut.CreateAsync("user-1", apiary.Id, "A1", "Dadant");

        Assert.That(hive.Id, Is.GreaterThan(0));
        Assert.That(hive.OwnerUserId, Is.EqualTo("user-1"));
        Assert.That(hive.ApiaryId, Is.EqualTo(apiary.Id));
        Assert.That(hive.Code, Is.EqualTo("A1"));
        Assert.That(hive.HiveType, Is.EqualTo("Dadant"));
    }

    [Test]
    public async Task GetForApiaryAsync_ReturnsOnlyOwnersHives()
    {
        using var db = DbContextFactory.Create(nameof(GetForApiaryAsync_ReturnsOnlyOwnersHives));
        var apiaryService = new ApiaryService(db);
        var sut = new HiveService(db);

        var apiary1 = await apiaryService.CreateAsync("user-1", "Bitynas 1", null);
        var apiary2 = await apiaryService.CreateAsync("user-2", "Bitynas 2", null);

        await sut.CreateAsync("user-1", apiary1.Id, "A1", null);
        await sut.CreateAsync("user-1", apiary1.Id, "A2", null);

        // kito user bitynas + avilys
        await sut.CreateAsync("user-2", apiary2.Id, "B1", null);

        var list = await sut.GetForApiaryAsync("user-1", apiary1.Id);

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.All(h => h.OwnerUserId == "user-1"), Is.True);
        Assert.That(list.All(h => h.ApiaryId == apiary1.Id), Is.True);
    }

    [Test]
    public async Task UpdateAsync_WhenNotOwner_ThrowsUnauthorized()
    {
        using var db = DbContextFactory.Create(nameof(UpdateAsync_WhenNotOwner_ThrowsUnauthorized));
        var apiaryService = new ApiaryService(db);
        var sut = new HiveService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);
        var hive = await sut.CreateAsync("user-1", apiary.Id, "A1", null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await sut.UpdateAsync(hive.Id, "user-2", "HACK", null));
    }

    [Test]
    public async Task DeleteAsync_WhenNotOwner_ThrowsUnauthorized()
    {
        using var db = DbContextFactory.Create(nameof(DeleteAsync_WhenNotOwner_ThrowsUnauthorized));
        var apiaryService = new ApiaryService(db);
        var sut = new HiveService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);
        var hive = await sut.CreateAsync("user-1", apiary.Id, "A1", null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await sut.DeleteAsync(hive.Id, "user-2"));
    }

    [Test]
    public async Task CreateAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_DuplicateName_ThrowsInvalidOperationException));

        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);

        await hiveService.CreateAsync("user-1", apiary.Id, "A1", null);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await hiveService.CreateAsync("user-1", apiary.Id, "A1", null));

        Assert.That(ex!.Message, Is.EqualTo("Toks avilio pavadinimas jau egzistuoja šiame bityne."));
    }

    [Test]
    public async Task UpdateAsync_ToExistingName_ThrowsInvalidOperationException()
    {
        using var db = DbContextFactory.Create(nameof(UpdateAsync_ToExistingName_ThrowsInvalidOperationException));

        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);

        var hive1 = await hiveService.CreateAsync("user-1", apiary.Id, "A1", null);
        var hive2 = await hiveService.CreateAsync("user-1", apiary.Id, "A2", null);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await hiveService.UpdateAsync(hive2.Id, "user-1", "A1", null));

        Assert.That(ex!.Message, Is.EqualTo("Toks avilio pavadinimas jau egzistuoja šiame bityne."));
    }

    [Test]
    public async Task CreateAsync_SameNameDifferentApiary_IsAllowed()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_SameNameDifferentApiary_IsAllowed));

        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);

        var apiary1 = await apiaryService.CreateAsync("user-1", "B1", null);
        var apiary2 = await apiaryService.CreateAsync("user-1", "B2", null);

        await hiveService.CreateAsync("user-1", apiary1.Id, "A1", null);

        // Tame pačiame useryje, bet kitame bityne – turi leisti
        var hive = await hiveService.CreateAsync("user-1", apiary2.Id, "A1", null);

        Assert.That(hive, Is.Not.Null);
        Assert.That(hive.ApiaryId, Is.EqualTo(apiary2.Id));
    }

    [Test]
    public async Task CreateAsync_SameNameDifferentUser_IsAllowed()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_SameNameDifferentUser_IsAllowed));

        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);

        var apiaryUser1 = await apiaryService.CreateAsync("user-1", "B1", null);
        var apiaryUser2 = await apiaryService.CreateAsync("user-2", "B2", null);

        await hiveService.CreateAsync("user-1", apiaryUser1.Id, "A1", null);

        // Skirtingas useris – turi leisti
        var hive = await hiveService.CreateAsync("user-2", apiaryUser2.Id, "A1", null);

        Assert.That(hive, Is.Not.Null);
        Assert.That(hive.OwnerUserId, Is.EqualTo("user-2"));
    }

}