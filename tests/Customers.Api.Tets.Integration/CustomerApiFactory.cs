using Customers.Api.Database;
using Customers.Api.Tests.Integration;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Customers.Api.Tets.Integration
{
    public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
    {
        //private readonly TestcontainersContainer _dbContainer =
        //    new TestcontainersBuilder<TestcontainersContainer>()
        //        .WithImage("postgres:latest")
        //        .WithEnvironment("POSTGRES_USER", "course")
        //        .WithEnvironment("POSTGRES_PASSWORD", "changeme")
        //        .WithEnvironment("POSTGRES_DB", "mydb")
        //        .WithPortBinding(5555, 5432)
        //        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        //        .Build();

        public const string VALID_GITHUB_USER = "igorsantanam";

        private readonly TestcontainerDatabase _dbContainer =
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new PostgreSqlTestcontainerConfiguration
                {
                    Database = "mydb",
                    Username = "course",
                    Password = "changeme"
                }).Build();

        private readonly GitHubApiServer _gitHubApiServer = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(IDbConnectionFactory));
                services.AddSingleton<IDbConnectionFactory>(_ =>
                    new NpgsqlConnectionFactory(_dbContainer.ConnectionString));
                services.AddHttpClient("GitHub", httpClient =>
                {
                    httpClient.BaseAddress = new Uri(_gitHubApiServer.Url);
                    httpClient.DefaultRequestHeaders.Add(
                        HeaderNames.Accept, "application/vnd.github.v3+json");
                    httpClient.DefaultRequestHeaders.Add(
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

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _dbContainer.DisposeAsync();
            _gitHubApiServer.Dispose(); 
        }
    }
}
