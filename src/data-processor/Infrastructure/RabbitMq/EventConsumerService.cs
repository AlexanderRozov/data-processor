using System.Text.Json;
using DataProcessor.Application.Events.Commands.CreateEvent;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DataProcessor.Infrastructure.RabbitMq;

internal class EventConsumerService : BackgroundService
{
    private readonly ILogger<EventConsumerService> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IMediator _mediator;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private IConnection? _connection;
    private IModel? _channel;

    public EventConsumerService(
        ILogger<EventConsumerService> logger,
        IOptions<RabbitMqOptions> options,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureChannel();
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, args) => await HandleMessage(args, stoppingToken);
        _channel!.BasicQos(0, 1, false);
        _channel.BasicConsume(queue: _options.QueueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("RabbitMQ consumer listening on {Queue}", _options.QueueName);
        return Task.CompletedTask;
    }

    private async Task HandleMessage(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var message = JsonSerializer.Deserialize<EventPayload>(args.Body.Span, _serializerOptions);
            if (message is null)
            {
                _logger.LogWarning("Received empty message. Acking to skip.");
                _channel?.BasicAck(args.DeliveryTag, multiple: false);
                return;
            }

            await _mediator.Send(new CreateEventCommand(message.Id, message.CreatedAt, message.Value), cancellationToken);
            _channel?.BasicAck(args.DeliveryTag, multiple: false);
            _logger.LogInformation("Stored event {EventId}", message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message");
            _channel?.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private void EnsureChannel()
    {
        if (_channel?.IsOpen == true)
        {
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection("data-processor");
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_options.QueueName, durable: true, exclusive: false, autoDelete: false);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ consumer");
        _channel?.Close();
        _connection?.Close();
        return base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }

    private record EventPayload(Guid Id, DateTime CreatedAt, int Value);
}

