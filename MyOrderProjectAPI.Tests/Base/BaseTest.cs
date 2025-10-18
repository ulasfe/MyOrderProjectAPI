using global::MyOrderProjectAPI.Data;
using global::MyOrderProjectAPI.DTOs;
using global::MyOrderProjectAPI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MyOrderProjectAPI.Tests.Base
{
    public class BaseTest : IDisposable
    {
        protected readonly ApplicationDbContext _context;
        private readonly SqliteConnection _connection;
        protected readonly IConfiguration _configuration;
        public BaseTest()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Database.EnsureCreated();
            _configuration = BuildConfiguration();
            TestDatabaseSeeder.Seed(_context);
        }

        private IConfiguration BuildConfiguration()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(currentDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }

      

        public void Dispose()
        {
            _context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }
}