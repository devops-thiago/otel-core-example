using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using UserApi.Data;

namespace UserApi.Tests
{
    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        private static int _databaseCounter = 0;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Create a unique database name for each test run
                var databaseName = $"TestDb_{Interlocked.Increment(ref _databaseCounter)}_{Guid.NewGuid()}";

                // Add InMemory database for testing
                services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName);
                });

                // Using custom JSON formatter to fix .NET 9 PipeWriter compatibility issues
            });
        }
    }
}
