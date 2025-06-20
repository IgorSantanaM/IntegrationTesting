using Customers.Api.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Testcontainers.PostgreSql;

namespace Customers.Api.Tests.Integration
{
    public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer =
            new PostgreSqlBuilder()
                .WithDatabase("db")
                .WithUsername("course")
                .WithPassword("whatever")
                .Build();

        public const string THROTTLED_USER = "throttle";

        private readonly GitHubApiServer _gitHubApiServer = new();

        public const string VALID_GITHUB_USER = "igorsantanam";
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IDbConnectionFactory));
                services.AddSingleton<IDbConnectionFactory>(_ =>
                    new NpgsqlConnectionFactory(_dbContainer.GetConnectionString()));

                services.AddHttpClient("GitHub", client =>
                {
                    client.BaseAddress = new Uri(_gitHubApiServer.Url);
                    client.DefaultRequestHeaders.Add(
                        HeaderNames.Accept, "application/vnd.github.v3+json");
                    client.DefaultRequestHeaders.Add(
                        HeaderNames.UserAgent, $"Course-{Environment.MachineName}");
                });
            });
        }

        public async Task InitializeAsync()
        {
            _gitHubApiServer.Start();
            _gitHubApiServer.SetupUser(VALID_GITHUB_USER);
            await _dbContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
            _gitHubApiServer.Dispose();
        }
    }
}
