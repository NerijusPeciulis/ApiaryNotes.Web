using ApiaryNotes.Tests.Helpers;
using ApiaryNotes.Web.Application.Services;
using NUnit.Framework;

namespace ApiaryNotes.Tests.Services;

[TestFixture]
public class ApiaryServiceTests
{
    [Test]
    public async Task CreateAsync_ValidInput_SavesApiaryWithOwner()
    {
        using var db = DbContextFactory.Create(nameof(CreateAsync_ValidInput_SavesApiaryWithOwner));
        var sut = new ApiaryService(db);

        var created = await sut.CreateAsync("user-1", "Mano bitynas", "Mažeikiai");

        Assert.That(created.Id, Is.GreaterThan(0));
        Assert.That(created.OwnerUserId, Is.EqualTo("user-1"));
        Assert.That(created.Name, Is.EqualTo("Mano bitynas"));
        Assert.That(created.Location, Is.EqualTo("Mažeikiai"));
    }

    [Test]
    public async Task GetForUserAsync_ReturnsOnlyUsersApiaries()
    {
        using var db = DbContextFactory.Create(nameof(GetForUserAsync_ReturnsOnlyUsersApiaries));
        var sut = new ApiaryService(db);

        await sut.CreateAsync("user-1", "A1", null);
        await sut.CreateAsync("user-1", "A2", null);
        await sut.CreateAsync("user-2", "B1", null);

        var list = await sut.GetForUserAsync("user-1");

        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(list.All(a => a.OwnerUserId == "user-1"), Is.True);
    }

    [Test]
    public async Task UpdateAsync_WhenNotOwner_ThrowsUnauthorized()
    {
        using var db = DbContextFactory.Create(nameof(UpdateAsync_WhenNotOwner_ThrowsUnauthorized));
        var sut = new ApiaryService(db);

        var created = await sut.CreateAsync("user-1", "A1", null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await sut.UpdateAsync(created.Id, "user-2", "Hacked", null));
    }

    [Test]
    public async Task DeleteAsync_WhenNotOwner_ThrowsUnauthorized()
    {
        using var db = DbContextFactory.Create(nameof(DeleteAsync_WhenNotOwner_ThrowsUnauthorized));
        var sut = new ApiaryService(db);

        var created = await sut.CreateAsync("user-1", "A1", null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await sut.DeleteAsync(created.Id, "user-2"));
    }
}