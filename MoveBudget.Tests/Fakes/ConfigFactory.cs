using Microsoft.Extensions.Configuration;

namespace MoveBudget.Tests.Fakes
{
    public static class ConfigFactory
    {
        public static IConfiguration CreateFakeConfig()
        {
            var settings = new Dictionary<string, string>
            {
                { "Jwt:Key", "MinhaChaveSuperSecreta123456" },
                { "Jwt:Issuer", "MoveBudgetAPI" },
                { "Jwt:Audience", "MoveBudgetAPI" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();
        }
    }
}