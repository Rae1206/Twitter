using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Twitter.Domain.Database.SqlServer.Context;

namespace Twitter.Domain.Database.SqlServer;

public class TwitterDbContextFactory : IDesignTimeDbContextFactory<TwitterDbContext>
{
    public TwitterDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TwitterDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TwitterDb;Trusted_Connection=True;");

        return new TwitterDbContext(optionsBuilder.Options);
    }
}
