using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var grainStorage = builder.AddParameter("EventStoreConfig-SirPixAlotGrainStorage"); 
var awsRegion = builder.AddParameter("AwsRegion");
var libsqlReplicaPath = builder.AddParameter("LibsqlDatabaseClientOptions-ReplicaPath");
var secretsPlaceHolder = builder.AddConnectionString("ConnectionString-PlaceHolder", "PlaceHolder");

var sirPixAlotWebAPI =  builder.AddProject<Projects.SirPixAlot_WebAPI>("SirPixAlotWebAPI");
sirPixAlotWebAPI.WithEnvironment("EventStoreConfig__Table", grainStorage);
sirPixAlotWebAPI.WithEnvironment("ConnectionString__PlaceHolder", secretsPlaceHolder);
sirPixAlotWebAPI.WithEnvironment("AWS_REGION", awsRegion);
sirPixAlotWebAPI.WithEnvironment("LibsqlDatabaseClientOptions__ReplicaPath", libsqlReplicaPath);

builder.Build().Run();
