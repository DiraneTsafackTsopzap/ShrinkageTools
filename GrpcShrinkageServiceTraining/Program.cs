using Dapper;
using DataAccess.CRUD.DapperContext;
using DataAccess.CRUD.Extensions;
using DataAccess.CRUD.Repositories;
using DataAccess.CRUD.Repositories.AbsencesRepository;
using DataAccess.CRUD.Repositories.TeamsRepository;
using DataAccess.CRUD.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Enregistrement du handler DateOnly pour Dapper (OBLIGATOIRE)
SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddScoped<IShrinkageUserRepository, ShrinkageUserRepository>();

// Teams Repository
builder.Services.AddScoped<IShrinkageTeamsRepository, ShrinkageTeamsRepository>();

// Absences Repository
builder.Services.AddScoped<IShrinkageAbsenceRepository, ShrinkageAbsenceRepository>();
builder.Services.AddScoped<DapperDbContext>();

var app = builder.Build();

// 🔥 Test de connexion PostgreSQL au démarrage
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DapperDbContext>();

    await using var conn =
        new NpgsqlConnection(dbContext.Connection.ConnectionString);

    await conn.OpenAsync();
    Console.WriteLine("✅ PostgreSQL connected");
}

app.MapGrpcService<ShrinkageUsersGrpcService>();

app.MapGet("/", () =>
    "Communication with gRPC endpoints must be made through a gRPC client. " +
    "To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
