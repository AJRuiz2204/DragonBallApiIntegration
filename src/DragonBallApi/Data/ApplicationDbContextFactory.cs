using DragonBallApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}