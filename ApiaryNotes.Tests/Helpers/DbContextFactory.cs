using ApiaryNotes.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiaryNotes.Tests.Helpers;

public static class DbContextFactory
{
    public static ApplicationDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }
}