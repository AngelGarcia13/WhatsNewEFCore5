using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

Console.WriteLine($"EF Core 5 Demo: TPT");
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
    var pokemons = new List<Pokemon>() {
        new Pokemon { DexNumber = "001", Name = "Bulbasaur" },
        new Pokemon { DexNumber = "004", Name = "Charmander" },
        new Pokemon { DexNumber = "007", Name = "Squirtle" },
        new Pokemon { DexNumber = "025", Name = "Pikachu" },
        new LegendaryPokemon { DexNumber = "144", Name = "Articuno", TipsForCatching = "I don't know"},
        new LegendaryPokemon { DexNumber = "145", Name = "Zapdos", TipsForCatching = "I don't know x2" },
        new LegendaryPokemon { DexNumber = "146", Name = "Moltres" , TipsForCatching = "I don't know x3"},
    };

    databaseContext.Pokemons.AddRange(pokemons);
    //Save Changes
    databaseContext.SaveChanges();

}

void QueryPokemons()
{
    foreach (var pokemon in databaseContext.Set<LegendaryPokemon>().Where(p => p.TipsForCatching.Contains("I don't know")).ToList())
    {
        Console.WriteLine($"You could catch {pokemon.Name} by doing the following: {pokemon.TipsForCatching}");
    }
}

public class PokemonsDbContext : DbContext
{

    public DbSet<Pokemon> Pokemons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
        .UseSqlite("Data Source=pokemons.db")
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);

    #region Map Table Per Hierarchy
    /*
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LegendaryPokemon>();
    }
    */
    #endregion

    #region Map Table Per Type

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LegendaryPokemon>().ToTable("LegendaryPokemons");
    }
    #endregion
}

public class Pokemon
{
    public int Id { get; set; }
    public string DexNumber { get; set; }
    public string Name { get; set; }

}

public class LegendaryPokemon : Pokemon
{
    public string TipsForCatching { get; set; }
}