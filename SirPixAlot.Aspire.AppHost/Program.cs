using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var grainStorage = builder.AddParameter("EventStoreConfig-SirPixAlotGrainStorage"); 
var awsRegion = builder.AddParameter("AwsRegion");
var libsqlReplicaPath = builder.AddParameter("LibsqlDatabaseClientOptions-ReplicaPath");
var secretsPlaceHolder = builder.AddConnectionString("PlaceHolder", "PlaceHolder");
var connectionStringSqlite = builder.AddConnectionString("Sqlite", "ConnectionStrings__Sqlite");

var sirPixAlotWebAPI =  builder.AddProject<Projects.SirPixAlot_WebAPI>("SirPixAlotWebAPI");
sirPixAlotWebAPI.WithEnvironment("EventStoreConfig__Table", grainStorage);
sirPixAlotWebAPI.WithEnvironment("ConnectionString__PlaceHolder", secretsPlaceHolder);
sirPixAlotWebAPI.WithEnvironment("AWS_REGION", awsRegion);
sirPixAlotWebAPI.WithEnvironment("LibsqlDatabaseClientOptions__ReplicaPath", libsqlReplicaPath);
sirPixAlotWebAPI.WithEnvironment("ConnectionStrings__Sqlite", connectionStringSqlite); 
sirPixAlotWebAPI.WithEndpoint(name: "orleans1", targetPort: 11111, isExternal: false);
sirPixAlotWebAPI.WithEndpoint(name: "orleans2",targetPort: 30000, isExternal: false);

builder.Build().Run();
