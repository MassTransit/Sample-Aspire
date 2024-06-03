using MassTransit;
using Npgsql;
using Sample.Components;
using Sample.Components.OrderManagement;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("postgres");

builder.Services.AddOptions<SqlTransportOptions>()
    .Configure(options =>
    {
        // this is temporary, later version of MT fixes this breakout from connection string
        var pg = new NpgsqlConnectionStringBuilder(connectionString);

        options.Host = pg.Host;
        options.Port = pg.Port;
        options.Username = pg.Username;
        options.Password = pg.Password;
        
        options.Database = "transport";
        options.Schema = "transport";
        options.Role = "transport";
    });

builder.Services.AddPostgresMigrationHostedService();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessOrderConsumer>();

    x.AddConfigureEndpointsCallback((provider, name, cfg) =>
    {
        if (cfg is ISqlReceiveEndpointConfigurator sql)
        {
            sql.LockDuration = TimeSpan.FromMinutes(10);
            sql.SetReceiveMode(SqlReceiveMode.PartitionedOrdered);
        }
    });

    x.AddSqlMessageScheduler();

    x.UsingPostgres((context, cfg) =>
    {
        cfg.UseSqlMessageScheduler();
        
        cfg.UsePublishFilter(typeof(CustomerNumberHeaderFilter<>), context);
        cfg.UseSendFilter(typeof(CustomerNumberHeaderFilter<>), context);
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();