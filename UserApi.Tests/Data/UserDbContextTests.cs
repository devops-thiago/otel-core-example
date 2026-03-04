using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.Models;
using Xunit;

namespace UserApi.Tests.Data;

public class UserDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<UserDbContext> _options;

    public UserDbContextTests()
    {
        // Use SQLite in-memory so that Database.ProviderName != "InMemory"
        // and the seed-data branch in OnModelCreating is exercised.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<UserDbContext>()
            .UseSqlite(_connection)
            .Options;
    }

    [Fact]
    public void OnModelCreating_WithSqliteProvider_ShouldSeedData()
    {
        // Arrange & Act
        using var context = new UserDbContext(_options);
        context.Database.EnsureCreated();

        // Assert — seed data should be present
        var users = context.Users.ToList();
        users.Should().HaveCount(2);

        var john = users.First(u => u.Id == 1);
        john.Name.Should().Be("John Doe");
        john.Email.Should().Be("john.doe@example.com");
        john.Bio.Should().Be("Software Engineer");

        var jane = users.First(u => u.Id == 2);
        jane.Name.Should().Be("Jane Smith");
        jane.Email.Should().Be("jane.smith@example.com");
        jane.Bio.Should().Be("Product Manager");
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureEmailAsUnique()
    {
        // Arrange
        using var context = new UserDbContext(_options);
        context.Database.EnsureCreated();
        // Clear seed data so we control all entries
        context.Users.RemoveRange(context.Users.ToList());
        context.SaveChanges();

        context.Users.Add(new User
        {
            Name = "User One",
            Email = "unique@example.com",
            Bio = "Bio",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.SaveChanges();

        context.Users.Add(new User
        {
            Name = "User Two",
            Email = "unique@example.com",
            Bio = "Bio",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Act & Assert — duplicate email should violate unique index
        var act = () => context.SaveChanges();
        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureEmailAsRequired()
    {
        // Arrange
        using var context = new UserDbContext(_options);
        context.Database.EnsureCreated();

        context.Users.Add(new User
        {
            Name = "User",
            Email = null!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Act & Assert
        var act = () => context.SaveChanges();
        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureNameAsRequired()
    {
        // Arrange
        using var context = new UserDbContext(_options);
        context.Database.EnsureCreated();

        context.Users.Add(new User
        {
            Name = null!,
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Act & Assert
        var act = () => context.SaveChanges();
        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void OnModelCreating_WithInMemoryProvider_ShouldNotSeedData()
    {
        // Arrange
        var inMemoryOptions = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act
        using var context = new UserDbContext(inMemoryOptions);
        context.Database.EnsureCreated();

        // Assert — InMemory provider should skip seed data
        var users = context.Users.ToList();
        users.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldInitializeDbSet()
    {
        // Arrange & Act
        using var context = new UserDbContext(_options);

        // Assert
        context.Users.Should().NotBeNull();
    }

    [Fact]
    public void OnModelCreating_ShouldConfigureIdAsPrimaryKey()
    {
        // Arrange
        using var context = new UserDbContext(_options);
        context.Database.EnsureCreated();
        // Clear seed data
        context.Users.RemoveRange(context.Users.ToList());
        context.SaveChanges();

        // Act
        var user = new User
        {
            Name = "Test",
            Email = "pk-test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        context.SaveChanges();

        // Assert — Id should be auto-generated
        user.Id.Should().BeGreaterThan(0);

        var loaded = context.Users.Find(user.Id);
        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Test");
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
