using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;
using Microsoft.Extensions.Options;


/*Service generates random number of events and sends them to RabbitMQ */
namespace DataGenerator;


internal sealed class DataGeneratorService : BackgroundService
{
    private readonly ILogger<DataGeneratorService> _logger;
    private readonly RabbitMqOptions _rabbitOptions;
    private readonly GeneratorOptions _generatorOptions;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private IConnection? _connection;
    private IModel? _channel;

    public DataGeneratorService(
        ILogger<DataGeneratorService> logger,
        IOptions<RabbitMqOptions> rabbitOptions,
        IOptions<GeneratorOptions> generatorOptions)
    {
        _logger = logger;
        _rabbitOptions = rabbitOptions.Value;
        _generatorOptions = generatorOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data generator started. Interval {Interval} ms", _generatorOptions.Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                EnsureChannel();

                var payload = GeneratedEvent.CreateRandom();
                var body = JsonSerializer.SerializeToUtf8Bytes(payload, _serializerOptions);

                var properties = _channel!.CreateBasicProperties();
                properties.ContentType = "application/json";
                properties.DeliveryMode = 2;

                _channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: _rabbitOptions.QueueName,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Published event {EventId} with value {Value}", payload.Id, payload.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event");
            }

            try
            {
                await Task.Delay(_generatorOptions.Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void EnsureChannel()
    {
        if (_channel?.IsOpen == true)
        {
            return;
        }

        _channel?.Dispose();
        _connection?.Dispose();

        var factory = new ConnectionFactory
        {
            HostName = _rabbitOptions.HostName,
            Port = _rabbitOptions.Port,
            UserName = _rabbitOptions.UserName,
            Password = _rabbitOptions.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection("data-generator");
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_rabbitOptions.QueueName, durable: true, exclusive: false, autoDelete: false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping data generator service");
        _channel?.Close();
        _connection?.Close();
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}

internal sealed record GeneratedEvent(Guid Id, DateTime CreatedAt, int Value)
{
    public static GeneratedEvent CreateRandom()
    {
        var value = Random.Shared.Next(0, 1000);
        return new GeneratedEvent(Guid.NewGuid(), DateTime.UtcNow, value);
    }
}

internal sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "events";
}

internal sealed class GeneratorOptions
{
    public int Interval { get; set; } = 2000;
}
