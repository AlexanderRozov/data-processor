using DataProcessor.Api;
using DataProcessor.Application.Common.Interfaces;
using DataProcessor.Application.Events.Commands.CreateEvent;
using DataProcessor.Infrastructure.Persistence;
using DataProcessor.Infrastructure.RabbitMq;
using DataProcessor.Migrations;
using FluentMigrator.Runner;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddMediatR(typeof(CreateEventCommand).Assembly);

builder.Services.Configure<DbOptions>(options =>
{
    options.ConnectionString =
        builder.Configuration["CONNECTION_STRING"] ??
        builder.Configuration.GetConnectionString("Default") ??
        "Host=localhost;Port=5432;Database=app_db;Username=app;Password=app";
});

builder.Services.Configure<RabbitMqOptions>(options =>
{
    options.HostName = builder.Configuration["RABBITMQ_HOST"] ?? "localhost";
    options.Port = int.TryParse(builder.Configuration["RABBITMQ_PORT"], out var port) ? port : 5672;
    options.UserName = builder.Configuration["RABBITMQ_USERNAME"] ?? "guest";
    options.Password = builder.Configuration["RABBITMQ_PASSWORD"] ?? "guest";
    options.QueueName = builder.Configuration["RABBITMQ_QUEUE"] ?? "events";
});

builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddHostedService<EventConsumerService>();

builder.Services
    .AddFluentMigratorCore()
    .ConfigureRunner(rb =>
    {
        rb.AddPostgres()
          .WithGlobalConnectionString(sp =>
          {
              var options = sp.GetRequiredService<IOptions<DbOptions>>();
              return options.Value.ConnectionString;
          })
          .ScanIn(typeof(CreateEventsTable).Assembly).For.Migrations();
    })
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var problem = Results.Problem(
            title: "Unexpected error",
            statusCode: StatusCodes.Status500InternalServerError,
            detail: feature?.Error.Message);
        await problem.ExecuteAsync(context);
    });
});
app.MapEventEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.Run();

public partial class Program;
