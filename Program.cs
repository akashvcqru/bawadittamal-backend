using Microsoft.EntityFrameworkCore;
using BawaDittaMal.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Database support based on DbProvider setting
var dbProvider = builder.Configuration["DbProvider"] ?? "Sqlite";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
    }
    else
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=bawadittamal.db");
    }
});

// Enable CORS for local development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed Database or Migrate Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    if (Array.Exists(args, arg => arg == "--migrate-db"))
    {
        Console.WriteLine("Starting data migration from local SQLite (bawadittamal.db) to remote SQL Server...");
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        DbSeeder.MigrateFromSqliteToSqlServer(context);
        Console.WriteLine("Data migration completed successfully!");
        return;
    }
    DbSeeder.Seed(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
