using Microsoft.EntityFrameworkCore;
using MoveBudget.API.Data;

namespace MoveBudget.Tests.Fakes
{
    public static class DbContextFactory
    {
        public static AppDbContext CreateInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;

            return new AppDbContext(options);
        }
    }
}