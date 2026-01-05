var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var docnestdb = postgres.AddDatabase("docnest-db");

builder.AddProject<Projects.DocNestApp_Api>("docnest-api")
    .WithReference(docnestdb)
    .WaitFor(postgres);
    
builder.AddProject<Projects.DocNestApp_Worker>("docnest-worker")
    .WithReference(docnestdb)
    .WaitFor(postgres);

builder.Build().Run();
