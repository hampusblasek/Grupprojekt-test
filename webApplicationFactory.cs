using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

public class ApplicationFactory<T> : WebApplicationFactory<T> where T : class
{
    // This method automaticly gets called on for every test.
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureTestServices(services =>
        {
            // Swaps the database for a test-database. Here Sqlite is beeing used.
            services.AddDbContext<Grupprojekt.AppDbContext>(options =>
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                options.UseSqlite($"Data Source={Path.Join(path, "ProgramTests.db")}");
            });

            // Swaps real authentication to "fake" authentication. Here fake tokens is created.
            // See class further down for more information.
            services.AddAuthentication("TestScheme")
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

            // Create / refere to the new test-DbContext
            var context = CreateDbContext(services);

            // Delete all data before every test
            context.Database.EnsureDeleted();

            // Create new tables again for every test.
            context.Database.EnsureCreated();

            // Create a user that can be used for testing.
            // This user use the same id as the authentication, so they get connected.
            CreateUser(context, "user-id");
        });
    }

    private static Grupprojekt.AppDbContext CreateDbContext(IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();

        var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<Grupprojekt.AppDbContext>();
        return context;
    }

    private static void CreateUser(Grupprojekt.AppDbContext db, string email)
    {
        // Creates an instance of a user
        var user = new Grupprojekt.User();
        //email and id for a user
        user.Email = "testuser@test.com";
        user.Id = "user-id";

        //Saves user to database
        db.Users.Add(user);

        // Register changes to database
        db.SaveChanges();
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // "AuthenticationHandler" requires some information that we pass along.
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    // Create a fake token with a user that has the ID "user-id".
    // It is linked to the user that is added to the database above.
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] {
            new Claim(ClaimTypes.Name, "my-user"),
            new Claim(ClaimTypes.NameIdentifier, "user-id")
        };
        
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}