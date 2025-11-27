using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace DataProcessor.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string RabbitUsername = "guest";
    private const string RabbitPassword = "guest";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("app_db")
        .WithUsername("app")
        .WithPassword("app")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithUsername(RabbitUsername)
        .WithPassword(RabbitPassword)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTesting");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["CONNECTION_STRING"] = _postgres.GetConnectionString(),
                ["RABBITMQ_HOST"] = _rabbitMq.Hostname,
                ["RABBITMQ_PORT"] = _rabbitMq.GetMappedPublicPort(5672).ToString(),
                ["RABBITMQ_USERNAME"] = RabbitUsername,
                ["RABBITMQ_PASSWORD"] = RabbitPassword
            };

            config.AddInMemoryCollection(overrides!);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbitMq.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _rabbitMq.DisposeAsync();
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}

