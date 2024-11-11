using Projects;
using Sample.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres",
        builder.CreateResourceBuilder(new ParameterResource("username", _ => "postgres")),
        builder.CreateResourceBuilder(new ParameterResource("password", _ => "Password12!")))
    .WithHealthCheck()
    .WithDataVolume()
    .WithPgAdmin();

var backEndDb = postgres.AddDatabase("sample", "sample");

// var username = builder.AddParameter("username", secret: false);
// var password = builder.AddParameter("password", secret: true);

var rabbitMq = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin();

var backEnd = builder.AddProject<Sample_BackEnd>("SampleBackEnd")
    .WithReference(backEndDb)
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq);

var api = builder.AddProject<Sample_Api>("SampleApi")
    .WithReference(backEndDb)
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(rabbitMq);

builder.Build().Run();