using Microsoft.Extensions.DependencyInjection;
using TanvirArjel.EFCore.GenericRepository;

namespace TestRepo.Data;

public static class RegisterService
{
    public static void AddRepository(
        this IServiceCollection service,
        string connectionString,
        bool isDocker
    )
    {
        if (isDocker)
            service.AddDbContext<MyAppContext>(opt => opt.UseSqlite(connectionString));
        else
            service.AddDbContext<MyAppContext>(opt => opt.UseNpgsql(connectionString));
        service.AddGenericRepository<MyAppContext>();
    }
}
