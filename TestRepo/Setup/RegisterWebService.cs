namespace TestRepo.Setup;

internal static class SetupWebApp
{
    internal static void RegisterService(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer().AddHttpContextAccessor();
        builder.Services.AddSwaggerGen();
        builder.Services.AddRepository(builder.Configuration.GetConnectionString("default")!);
        builder
            .Services
            .AddTransient(p =>
            {
                var logFactory = p.GetRequiredService<ILoggerFactory>();
                var httpContext = p.GetRequiredService<IHttpContextAccessor>().HttpContext!;
                return logFactory.CreateLogger(httpContext.Request.Path);
            });
    }

    internal static async Task StartupAction(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        await app.InitialDb();
    }
}
