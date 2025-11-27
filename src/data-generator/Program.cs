using DataGenerator;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<RabbitMqOptions>(options =>
{
    options.HostName = builder.Configuration["RABBITMQ_HOST"] ?? builder.Configuration["RabbitMq:HostName"] ?? "localhost";
    options.Port = int.TryParse(builder.Configuration["RABBITMQ_PORT"], out var port)
        ? port
        : int.TryParse(builder.Configuration["RabbitMq:Port"], out var configPort) ? configPort : 5672;
    options.UserName = builder.Configuration["RABBITMQ_USERNAME"] ?? builder.Configuration["RabbitMq:UserName"] ?? "guest";
    options.Password = builder.Configuration["RABBITMQ_PASSWORD"] ?? builder.Configuration["RabbitMq:Password"] ?? "guest";
    options.QueueName = builder.Configuration["RABBITMQ_QUEUE"] ?? builder.Configuration["RabbitMq:QueueName"] ?? "events";
});

builder.Services.Configure<GeneratorOptions>(options =>
{
    options.Interval = int.TryParse(builder.Configuration["GENERATOR_INTERVAL_MS"], out var interval)
        ? interval
        : builder.Configuration.GetValue<int?>("Generator:IntervalMs") ?? 2000;
});

builder.Services.AddHostedService<DataGeneratorService>();

var host = builder.Build();
host.Run();
