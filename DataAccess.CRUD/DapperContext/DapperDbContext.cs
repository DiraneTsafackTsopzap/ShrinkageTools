using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace DataAccess.CRUD.DapperContext
{
    public class DapperDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _connection;

        public DapperDbContext(IConfiguration configuration)
        {
            _configuration = configuration;

            string template = _configuration.GetConnectionString("PostGresConnection")!;

            string host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? throw new Exception("POSTGRES_HOST not set");
            string port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
            string db = Environment.GetEnvironmentVariable("POSTGRES_DATABASE") ?? throw new Exception("POSTGRES_DATABASE not set");
            string user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? throw new Exception("POSTGRES_USER not set");
            string pwd = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? throw new Exception("POSTGRES_PASSWORD not set");

            string final = template
                .Replace("$POSTGRES_HOST", host)
                .Replace("$POSTGRES_PORT", port)
                .Replace("$POSTGRES_DATABASE", db)
                .Replace("$POSTGRES_USER", user)
                .Replace("$POSTGRES_PASSWORD", pwd);

            _connection = new NpgsqlConnection(final);
           // Console.WriteLine("ConnectionString : " + final);
        }


        public IDbConnection Connection => _connection;


    }
}
