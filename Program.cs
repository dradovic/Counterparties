// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;

using (var context = new AppDbContext())
{
    context.Database.Migrate(); // apply any outstanding migrations

    context.Counterparties.ExecuteDelete();
    context.Counterparties.Add(new CorporateCounterparty { CompanyName = "Lego" });
    context.Counterparties.Add(new PrivateCounterparty { PersonName = "Charles" });
    context.SaveChanges();
}

using (var context = new AppDbContext())
{
    context.Contracts.RemoveRange(context.Contracts);
    context.Contracts.Add(new CreditContract
    {
        Counterparties = [
            context.Counterparties.OfType<CorporateCounterparty>().Single(),
            context.Counterparties.OfType<PrivateCounterparty>().Single(),
        ],
    });
    context.SaveChanges();
    Console.WriteLine("Stored.");
}

using (var context = new AppDbContext())
{
    foreach (var contract in context.Contracts)
    {
        Console.WriteLine(contract.ToString());
    }
}

public class CreditContract
{
    public int Id { get; set; }

    public ICollection<Counterparty> Counterparties { get; set; } = null!;

    public override string ToString()
    {
        return string.Join(", ", Counterparties.Select(c => c.ToString()));
    }
}

public abstract class Counterparty
{
    public int Id { get; set; }

    #region Many-to-many mappings

    public ICollection<CreditContract> InCreditContracts { get; set; } = null!;

    #endregion
}

public class PrivateCounterparty : Counterparty
{
    public string? PersonName { get; set; }

    public override string ToString()
    {
        return $"Private: {PersonName}";
    }
}

public class CorporateCounterparty : Counterparty
{
    public string? CompanyName { get; set; }

    public override string ToString()
    {
        return $"Corp: {CompanyName}";
    }
}

public class AppDbContext : DbContext
{
    public DbSet<Counterparty> Counterparties { get; init; }
    public DbSet<CreditContract> Contracts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Counterparty>()
            .HasMany(x => x.InCreditContracts)
            .WithMany(x => x.Counterparties)
            .UsingEntity<Dictionary<string, object>>("CreditContractCounterparties", j => j.HasOne<CreditContract>().WithMany().HasForeignKey("ContractId"), j => j.HasOne<Counterparty>().WithMany().HasForeignKey("CounterpartyId"));
        builder.Entity<CorporateCounterparty>()
            .HasBaseType<Counterparty>();
        //builder.Entity<CorporateCounterparty>()
        //    .Navigation(x => x.Company)
        //    .AutoInclude();
        builder.Entity<PrivateCounterparty>()
            .HasBaseType<Counterparty>();
        //builder.Entity<PrivateCounterparty>()
        //    .Navigation(x => x.Person)
        //    .AutoInclude();
        builder.Entity<CreditContract>()
            .Navigation(x => x.Counterparties)
            .AutoInclude();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=Counterparties;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False");
    }
}
