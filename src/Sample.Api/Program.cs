using MassTransit;
using Sample.Components;
using Sample.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddServiceDefaults();

// var connectionString = builder.Configuration.GetConnectionString("sample");
//
// builder.Services.AddOptions<SqlTransportOptions>()
//     .Configure(options =>
//     {
//         options.ConnectionString = connectionString;
//     });
//
builder.Services.AddMassTransit(x =>
{
//    x.AddSqlMessageScheduler();
x.AddDelayedMessageScheduler();

    x.UsingRabbitMq((context, cfg) =>
    //x.UsingPostgres((context, cfg) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("messaging");
        cfg.Host(connectionString);
        
        //cfg.UseSqlMessageScheduler();
        cfg.UseDelayedMessageScheduler();

        cfg.UsePublishFilter(typeof(CustomerNumberPartitionKeyFilter<>), context);
        cfg.UseSendFilter(typeof(CustomerNumberPartitionKeyFilter<>), context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

app.MapPost("/order", async (OrderModel order, IPublishEndpoint publishEndpoint) =>
    {
        await publishEndpoint.Publish(new ProcessOrder
        {
            OrderId = order.OrderId,
            CustomerNumber = order.CustomerNumber
        });

        return Results.Ok(new OrderInfoModel(order.OrderId, DateTime.UtcNow));
    })
    .WithName("ProcessOrder")
    .WithOpenApi();

app.Run();