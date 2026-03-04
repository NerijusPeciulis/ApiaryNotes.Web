using ApiaryNotes.Tests.Helpers;
using ApiaryNotes.Web.Application.Services;
using ApiaryNotes.Web.Domain;
using NUnit.Framework;

namespace ApiaryNotes.Tests.Services;

[TestFixture]
public class HarvestServiceTests
{
    [Test]
    public async Task CreateAsync_WhenHiveNotFound_ThrowsKeyNotFound()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_WhenHiveNotFound_ThrowsKeyNotFound));
        var sut = new HarvestService(db);

        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await sut.CreateAsync("user-1", hiveId: 999, new DateOnly(2025, 12, 5), 15m, HarvestUnit.Kg, null));
    }

    [Test]
    public async Task CreateAsync_ValidInput_SavesHarvest()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_ValidInput_SavesHarvest));
        var apiaryService = new ApiaryService(db);
        var hiveService = new HiveService(db);
        var sut = new HarvestService(db);

        var apiary = await apiaryService.CreateAsync("user-1", "Bitynas", null);
        var hive = await hiveService.CreateAsync("user-1", apiary.Id, "A1", null);

        var harvest = await sut.CreateAsync("user-1", hive.Id, new DateOnly(2025, 12, 5), 15m, HarvestUnit.L, "Išimta po sukimų");

        Assert.That(harvest.Id, Is.GreaterThan(0));
        Assert.That(harvest.OwnerUserId, Is.EqualTo("user-1"));
        Assert.That(harvest.HiveId, Is.EqualTo(hive.Id));
        Assert.That(harvest.Amount, Is.EqualTo(15m));
        Assert.That(harvest.Unit, Is.EqualTo(HarvestUnit.L));
    }
}