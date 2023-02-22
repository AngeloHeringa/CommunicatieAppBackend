using CommunicatieAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend{
public class AppDbContext : DbContext{
    public DbSet<Melding> meldingen {get;set;}
    public DbSet<Nieuwsbericht> nieuwsberichten {get;set;}
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql("server=localhost;port=3306;user=root;password=Star123;database=CommunicatieApp",
                new MariaDbServerVersion(new Version(10, 9, 3)))
            // TODO WHEN DEPLOYING REMOVE THIS!
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Melding>().HasData(
            new Melding{
                MeldingId=1, Titel="test", Inhoud="hoi", Datum=DateTime.Now
            },
            new Melding{
                MeldingId=2, Titel="test2", Inhoud="hoi2", Datum=DateTime.Now
            }
        );
        modelBuilder.Entity<Nieuwsbericht>().HasData(
            new Nieuwsbericht{
                NieuwsberichtId=1, Titel="test", Inhoud="hoi", Datum=DateTime.Now
            },
            new Nieuwsbericht{
                NieuwsberichtId=2, Titel="test2", Inhoud="hoi2", Datum=DateTime.Now
            }
        );
    }
}}