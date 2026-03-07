using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ApiaryNotes.Web.Domain;

namespace ApiaryNotes.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Apiary> Apiaries => Set<Apiary>();
    public DbSet<Hive> Hives => Set<Hive>();
    public DbSet<HiveNote> HiveNotes => Set<HiveNote>();
    public DbSet<HiveHarvest> HiveHarvests => Set<HiveHarvest>();
    public DbSet<FinanceEntry> FinanceEntries => Set<FinanceEntry>();
}
