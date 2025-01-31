using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WgNode.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy( policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1");
builder.Services.AddLogging();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddScoped<IPeerService,PeerService>();
builder.Services.AddScoped<IServerService, ServerService>();
builder.Services.AddScoped<ICommandExecutor, CommandExecutor>();
builder.Services.AddSingleton<IServerStorage, ServerStorage>();

builder.Services.AddDbContext<MainDbContext>(opt =>
    opt.UseSqlite("Data Source=/etc/wireguard/database.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.MapScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.Mars)
        .WithDarkMode(true)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .WithDarkModeToggle(false);
});

using (var serviceScope = app.Services.CreateScope())
{
    var server = serviceScope.ServiceProvider.GetRequiredService<IServerService>();
    var db = serviceScope.ServiceProvider.GetRequiredService<MainDbContext>();
    
    db.Database.Migrate();
    
    if (db.Servers.Any()) await server.LoadServer();
    else await server.CreateServer(Environment.GetEnvironmentVariable("WgHost")??String.Empty, Convert.ToInt32(Environment.GetEnvironmentVariable("WgPort")));
    
    await server.LaunchServer();
}



app.Run();