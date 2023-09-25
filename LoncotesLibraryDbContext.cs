using Microsoft.EntityFrameworkCore;
using LoncotesLibrary.Models;

public class LoncotesLibraryDbContext : DbContext
{
    public DbSet<Genre> Genres {get; set; }
    public DbSet<Material> Materials {get; set; }
    public DbSet<MaterialType> MaterialTypes {get; set; }
    public DbSet<Patron> Patrons {get; set; }
    public DbSet<Checkout> Checkouts {get; set; }

    public LoncotesLibraryDbContext(DbContextOptions<LoncotesLibraryDbContext> context) : base(context)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaterialType>().HasData(new MaterialType[]
        {
            new MaterialType {Id = 1, Name = "Book", CheckoutDays = 15},
            new MaterialType {Id = 2, Name = "CD", CheckoutDays = 10},
            new MaterialType {Id = 3, Name = "Game", CheckoutDays = 3}
        });

        modelBuilder.Entity<Patron>().HasData(new Patron[]
        {
            new Patron {Id = 1, FirstName = "Jill", LastName = "Valentine", Address = "Raccoon City", Email = "ChrisIsMyBoyfriend@sneeble.fart", IsActive = true},
            new Patron {Id = 2, FirstName = "Big", LastName = "Boss", Address = "Outer Haven", Email = "BossWhyDidYouDefect@sad.boi", IsActive = true}
        });

        modelBuilder.Entity<Genre>().HasData(new Genre[]
        {
            new Genre {Id = 1, Name = "SciFi"},
            new Genre {Id = 2, Name = "Fantasy"},
            new Genre {Id = 3, Name = "Historical"},
            new Genre {Id = 4, Name = "Horror"},
            new Genre {Id = 5, Name = "Romance"}
        });

        modelBuilder.Entity<Material>().HasData(new Material[]
        {
            new Material {Id = 1, GenreId = 3, MaterialTypeId = 2, MaterialName = "History of Japan: The Second World War"},
            new Material {Id = 2, GenreId = 4, MaterialTypeId = 1, MaterialName = "Misery"},
            new Material {Id = 3, GenreId = 2, MaterialTypeId = 1, MaterialName = "Blood of Elves"},
            new Material {Id = 4, GenreId = 4, MaterialTypeId = 3, MaterialName = "Silent Hill"},
            new Material {Id = 5, GenreId = 5, MaterialTypeId = 1, MaterialName = "Twilight: Extra Sparkle Edition"},
            new Material {Id = 6, GenreId = 3, MaterialTypeId = 2, MaterialName = "Romance of the Sengoku Era"},
            new Material {Id = 7, GenreId = 4, MaterialTypeId = 2, MaterialName = "Halloween Music Compilation"},
            new Material {Id = 8, GenreId = 3, MaterialTypeId = 3, MaterialName = "Romance of the Three Kingdoms"},
            new Material {Id = 9, GenreId = 1, MaterialTypeId = 1, MaterialName = "The Horus Herasey"},
            new Material {Id = 10, GenreId = 1, MaterialTypeId = 2, MaterialName = "Warhammer 40,000: The Black Library"}
        });
    }
}