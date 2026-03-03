using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApiaryNotes.Web.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // tas pats kaip appsettings.json: Data Source=apiarynotes.db
        optionsBuilder.UseSqlite("Data Source=apiarynotes.db");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}