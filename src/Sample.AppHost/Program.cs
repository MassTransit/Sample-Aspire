using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres",
        builder.CreateResourceBuilder(new ParameterResource("username", _ => "postgres")),
        builder.CreateResourceBuilder(new ParameterResource("password", _ => "Password12!")))
    .WithHealthCheck()
    .WithDataVolume()
    .WithPgAdmin();

var backEndDb = postgres.AddDatabase("sample", "sample");

var backEnd = builder.AddProject<Sample_BackEnd>("SampleBackEnd")
    .WithReference(backEndDb)
    .WaitFor(postgres);

var api = builder.AddProject<Sample_Api>("SampleApi")
    .WithReference(backEndDb)
    .WaitFor(postgres);

builder.Build().Run();