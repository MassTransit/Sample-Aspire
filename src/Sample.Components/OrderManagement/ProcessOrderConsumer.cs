namespace Sample.Components.OrderManagement;

using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;


public class ProcessOrderConsumer :
    IConsumer<ProcessOrder>
{
    readonly ILogger<ProcessOrderConsumer> _logger;

    public ProcessOrderConsumer(ILogger<ProcessOrderConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProcessOrder> context)
    {
        _logger.LogInformation("Processing Order {OrderId} for {CustomerNumber}", context.Message.OrderId, context.Message.CustomerNumber);

        return Task.CompletedTask;
    }
}
