using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var grainStorage = builder.AddParameter("EventStoreConfig-SirPixAlotGrainStorage");
var awsRegion = builder.AddParameter("AwsRegion");
var placeHolder = builder.AddConnectionString("ConnectionString-PlaceHolder", "PlaceHolder");

var sirPixAlotWebAPI =  builder.AddProject<Projects.SirPixAlot_WebAPI>("SirPixAlotWebAPI");
sirPixAlotWebAPI.WithEnvironment("EventStoreConfig__SirPixAlotGrainStorage", grainStorage);
sirPixAlotWebAPI.WithEnvironment("ConnectionString__PlaceHolder", placeHolder);
sirPixAlotWebAPI.WithEnvironment("AWS_REGION", awsRegion);

builder.Build().Run();
