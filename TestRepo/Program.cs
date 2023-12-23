using TestRepo.Routes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

await app.InitialDb();

app.MapGroup("/person").WithTags("Person").HandlePersonRoute();

app.Run();
