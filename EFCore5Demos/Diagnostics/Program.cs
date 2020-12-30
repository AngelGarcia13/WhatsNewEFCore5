using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

Console.WriteLine($"EF Core 5 Demo: Diagnostics | Process ID: {Process.GetCurrentProcess().Id}");
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
        new Pokemon { DexNumber = "025", Name = "Pikachu" }
    };

    databaseContext.Pokemons.AddRange(pokemons);

    #region Initialize Events
    
   //Add Events to the SaveChanges method
   AddSaveChangesEvents();
   
    #endregion

    //Save Changes
    databaseContext.SaveChanges();

}

void QueryPokemons() {
    foreach (var pokemon in databaseContext.Pokemons.ToList())
    {
        Console.WriteLine($"{pokemon.Name} is {pokemon.DexNumber}");
    }
}

#region Events
void AddSaveChangesEvents()
{
    databaseContext.SavingChanges += (sender, args) =>
    {
        Console.WriteLine($"Saving changes for {((DbContext)sender).Database.GetConnectionString()}");
    };

    databaseContext.SavedChanges += (sender, args) =>
    {
        Console.WriteLine($"Saved {args.EntitiesSavedCount} changes for {((DbContext)sender).Database.GetConnectionString()}");
    };
}
#endregion


#region Interceptors
public class MySaveChangesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Console.WriteLine($"Saving changes for {eventData.Context.Database.GetConnectionString()}");

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        Console.WriteLine($"Saving changes asynchronously for {eventData.Context.Database.GetConnectionString()}");

        return new ValueTask<InterceptionResult<int>>(result);
    }
}

#endregion

public class PokemonsDbContext : DbContext {

    public DbSet<Pokemon> Pokemons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
        .UseSqlite("Data Source=pokemons.db")
        .AddInterceptors(new MySaveChangesInterceptor())
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);

    #region Map Entity To Query
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pokemon>().ToSqlQuery(
            @"SELECT Id, DexNumber, Name FROM pokemons WHERE DexNumber != '025'");
    }
    
    #endregion
}

public class Pokemon
{
    public int Id { get; set; }
    public string DexNumber { get; set; }
    public string Name { get; set; }

}