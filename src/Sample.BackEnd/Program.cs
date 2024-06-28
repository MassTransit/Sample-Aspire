using MassTransit;
using Npgsql;
using Sample.Components;
using Sample.Components.OrderManagement;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("sample");

builder.Services.AddOptions<SqlTransportOptions>()
    .Configure(options =>
    {
        options.ConnectionString = connectionString;
    });

builder.Services.AddPostgresMigrationHostedService(options =>
{
    // default, but shown for completeness
    options.CreateDatabase = true;
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessOrderConsumer>();

    x.AddConfigureEndpointsCallback((provider, name, cfg) =>
    {
        if (cfg is ISqlReceiveEndpointConfigurator sql)
        {
            sql.LockDuration = TimeSpan.FromMinutes(10);

            // Ensure messages are consumed in order within a partition
            // Prevents head-of-line blocking across customers
            sql.SetReceiveMode(SqlReceiveMode.PartitionedOrdered);
        }
    });

    x.AddSqlMessageScheduler();

    x.UsingPostgres((context, cfg) =>
    {
        cfg.UseSqlMessageScheduler();

        cfg.UsePublishFilter(typeof(CustomerNumberPartitionKeyFilter<>), context);
        cfg.UseSendFilter(typeof(CustomerNumberPartitionKeyFilter<>), context);
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();