using Projects;
using Sample.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres",
        builder.CreateResourceBuilder(new ParameterResource("username", _ => "postgres")),
        builder.CreateResourceBuilder(new ParameterResource("password", _ => "Password12!")))
    .WithHealthCheck()
    .WithDataVolume()
    .WithPgAdmin();

var backEnd = builder.AddProject<Sample_BackEnd>("SampleBackEnd")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.Build().Run();