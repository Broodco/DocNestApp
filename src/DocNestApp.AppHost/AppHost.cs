var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var docnestdb = postgres.AddDatabase("docnest-db");

var api = builder.AddProject<Projects.DocNestApp_Api>("docnest-api")
    .WithReference(docnestdb)
    .WithHttpEndpoint(name: "api")
    .WaitFor(postgres);

builder.AddProject<Projects.DocNestApp_Worker>("docnest-worker")
    .WithReference(docnestdb)
    .WaitFor(postgres);

builder.AddProject<Projects.DocNestApp_Web>("docnest-web")
    .WithReference(api)
    .WithHttpEndpoint(name: "web", port: 8080)
    .WithEnvironment("DocNest__ApiBaseUrl", api.GetEndpoint("api"));

builder.Build().Run();