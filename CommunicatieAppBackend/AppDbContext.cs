using CommunicatieAppBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunicatieAppBackend{
public class AppDbContext : DbContext{
    public DbSet<Melding> meldingen {get;set;}
    public DbSet<Nieuwsbericht> nieuwsberichten {get;set;}
    public DbSet<Locatie> Locaties {get;set;}
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
        modelBuilder.Entity<Locatie>().HasData(
            new Locatie{
                Id=1,
                name="Rijnland"
            },
            new Locatie{
                Id=2,
                name="Schieland"
            }
        );

        modelBuilder.Entity<Melding>().HasData(
            new Melding{
                MeldingId=1, Titel="Besluit Centraal Stembureau kandidaatlijsten", Inhoud="In navolging van het besluit dat het Centraal Stembureau 3 februari jl. heeft genomen is er geen bezwaar noch beroep hiertegen aangetekend. Dit houdt in dat de Kandidatenlijsten definitief zijn.Op vrijdag 3 februari heeft het Centraal Stembureau het besluit genomen over de geldigheid en nummering van de kandidatenlijsten voor de waterschapsverkiezingen op 15 maart 2023.", Datum=DateTime.Now, LocatieId=1
            },
            new Melding{
                MeldingId=2, Titel="Besluit Centraal Stembureau kandidaatlijsten", Inhoud="In navolging van het besluit dat het Centraal Stembureau 3 februari jl. heeft genomen is er geen bezwaar noch beroep hiertegen aangetekend. Dit houdt in dat de Kandidatenlijsten definitief zijn.Op vrijdag 3 februari heeft het Centraal Stembureau het besluit genomen over de geldigheid en nummering van de kandidatenlijsten voor de waterschapsverkiezingen op 15 maart 2023.", Datum=DateTime.Now, LocatieId=1
            }
        );
        modelBuilder.Entity<Nieuwsbericht>().HasData(
            new Nieuwsbericht{
                NieuwsberichtId=1, Titel="Verkiezingen, wat kun jij doen…..?!", Inhoud="Het is bijna weer zover, één keer in de vier jaar vieren we het feest van de democratie voor het waterschap via verkiezingen. Daar gaat een uitgekiende campagne bij helpen. Met de campagne maken we de inwoners van Rijnland nog meer bewust van het belangrijke werk dat wij doen. En vooral de bijzondere rol die zijzelf hebben, namelijk stemmen. En ja helaas, dat is in deze tijd nog steeds een bijzonder en groot goed dat we met elkaar moeten koesteren!!", Datum=DateTime.Now, Image="plaatje.jpg", LocatieId=1
            },
            new Nieuwsbericht{
                NieuwsberichtId=2, Titel="Verkiezingen, wat kun jij doen…..?!", Inhoud="Het is bijna weer zover, één keer in de vier jaar vieren we het feest van de democratie voor het waterschap via verkiezingen. Daar gaat een uitgekiende campagne bij helpen. Met de campagne maken we de inwoners van Rijnland nog meer bewust van het belangrijke werk dat wij doen. En vooral de bijzondere rol die zijzelf hebben, namelijk stemmen. En ja helaas, dat is in deze tijd nog steeds een bijzonder en groot goed dat we met elkaar moeten koesteren!!", Datum=DateTime.Now, Image="plaatje.jpg", LocatieId=1
            }
        );

    }
}}