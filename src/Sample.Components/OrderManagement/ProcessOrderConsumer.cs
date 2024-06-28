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
        _logger.LogInformation("Processing {CustomerNumber,-20} Order {OrderId}", context.Message.CustomerNumber, context.Message.OrderId);

        return Task.CompletedTask;
    }
}