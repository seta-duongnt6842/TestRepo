namespace TestRepo.Utils;

public static class BootstrapDb
{
    public static async Task InitialDb(this WebApplication app)
    {
        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
            var context = scope.ServiceProvider.GetRequiredService<MyAppContext>();
            await context.Database.EnsureCreatedAsync();
            if (!await repository.ExistsAsync<Person>())
            {
                await context.BulkInsertAsync(SeedData.GetPeople());
            }
        }
        catch (Exception ex)
        {
            app.Logger.InitializeDatabaseFail(ex.GetBaseException().Message);
        }
    }
}
