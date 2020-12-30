using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

Console.WriteLine($"EF Core 5 Demo: Many To Many");
//Global reference to the db context
using var databaseContext = new PokemonsDbContext();

//Setup the db and insert some data
InitializeDatabase();

//Query the db
QueryPokemons();

Console.Write("Press Any Key To Terminate...");
Console.ReadKey();

void InitializeDatabase()
{
    //Ensure to remove the db (if exists)
    databaseContext.Database.EnsureDeleted();

    //Create the db
    databaseContext.Database.EnsureCreated();

    //Insert some rows
    var charmander = new Pokemon { DexNumber = "004", Name = "Charmander" };
    var pikachu = new Pokemon { DexNumber = "025", Name = "Pikachu" };
    var zapdos = new Pokemon { DexNumber = "145", Name = "Zapdos" };
    var moltres = new Pokemon { DexNumber = "146", Name = "Moltres" };

    var fire = new Type { Name = "Fire", Pokemons = new List<Pokemon> { charmander, moltres } };
    var electric = new Type { Name = "Electric", Pokemons = new List<Pokemon> { pikachu, zapdos } };
    var flying = new Type { Name = "Flying", Pokemons = new List<Pokemon> { zapdos, moltres } };

    databaseContext.AddRange(
        charmander, pikachu, zapdos,
        moltres, fire, electric, flying);

    //Save Changes
    databaseContext.SaveChanges();

}

void QueryPokemons()
{
    var query = databaseContext.Pokemons.Where(p => p.PokemonTypeMemberships.Any(m => m.Level > 1)).ToList();
    foreach (var pokemon in query)
    {
        Console.WriteLine($"{pokemon.Name} is {pokemon.DexNumber}");
    }
}

public class PokemonsDbContext : DbContext
{

    public DbSet<Pokemon> Pokemons { get; set; }
    public DbSet<Type> Types { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
        .UseSqlite("Data Source=pokemons.db")
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pokemon>()
            .HasMany(p => p.Types)
            .WithMany(t => t.Pokemons)
            .UsingEntity<PokemonTypeMembership>(
                pt => pt.HasOne(m => m.Type).WithMany(g => g.PokemonTypeMemberships),
                pt => pt.HasOne(m => m.Pokemon).WithMany(g => g.PokemonTypeMemberships)
            );
    }

}


public class Pokemon
{
    public int Id { get; set; }
    public string DexNumber { get; set; }
    public string Name { get; set; }
    public ICollection<Type> Types { get; set; }
    public ICollection<PokemonTypeMembership> PokemonTypeMemberships { get; set; }
}

public class Type
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Pokemon> Pokemons { get; set; }
    public ICollection<PokemonTypeMembership> PokemonTypeMemberships { get; set; }
}

public class PokemonTypeMembership
{
    public Pokemon Pokemon { get; set; }
    public Type Type { get; set; }
    public int Level { get; set; }
}
