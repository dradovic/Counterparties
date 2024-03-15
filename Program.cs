// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;

using (var context = new AppDbContext())
{
    context.Database.Migrate(); // apply any outstanding migrations

    context.Contracts.Add(new CreditContract
    {
        Counterparties = [
            new CorporateCreditCounterparty { CompanyName = "Lego" },
            new PrivateCreditCounterparty { PersonName = "Charles" }
        ],
    });
    context.SaveChanges();
    Console.WriteLine("Stored.");
}

using (var context = new AppDbContext())
{
    foreach (var contract in context.Contracts.Include(c => c.Counterparties))
    {
        Console.WriteLine(contract.ToString());
    }
}

public class CreditContract
{
    public int Id { get; set; }

    public ICollection<CreditCounterparty> Counterparties { get; set; } = null!;

    public override string ToString()
    {
        return string.Join(", ", Counterparties.Select(c => c.ToString()));
    }
}

public abstract class CreditCounterparty
{
    public int Id { get; set; }
    public int CreditContractId { get; set; }
}

public class PrivateCreditCounterparty : CreditCounterparty
{
    public string? PersonName { get; set; }

    public override string ToString()
    {
        return $"Private: {PersonName}";
    }
}

public class CorporateCreditCounterparty : CreditCounterparty
{
    public string? CompanyName { get; set; }

    public override string ToString()
    {
        return $"Corp: {CompanyName}";
    }
}

public class AppDbContext : DbContext
{
    public DbSet<CreditContract> Contracts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<CorporateCreditCounterparty>()
            .HasBaseType<CreditCounterparty>();
        builder.Entity<PrivateCreditCounterparty>()
            .HasBaseType<CreditCounterparty>();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=PolymorphicCollection;Trusted_Connection=True;MultipleActiveResultSets=True;Encrypt=False");
    }
}
