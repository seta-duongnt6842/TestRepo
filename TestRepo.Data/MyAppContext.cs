using TestRepo.Data.Entities;

namespace TestRepo.Data;

public class MyAppContext(DbContextOptions<MyAppContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Account> Accounts => Set<Account>();
}
