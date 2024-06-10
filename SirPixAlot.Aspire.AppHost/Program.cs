using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var grainStorage = builder.AddParameter("EventStoreConfig-SirPixAlotGrainStorage");
var awsAccessKeyId = builder.AddParameter("AwsAccessKeyId", secret: true);
var awsSecretAccessKey = builder.AddParameter("AwsSecretAccessKey", secret: true);

var sirPixAlotWebAPI =  builder.AddProject<Projects.SirPixAlot_WebAPI>("SirPixAlotWebAPI");
sirPixAlotWebAPI.WithEnvironment("EventStoreConfig__SirPixAlotGrainStorage", grainStorage);
sirPixAlotWebAPI.WithEnvironment("AWS_ACCESS_KEY_ID", awsAccessKeyId);
sirPixAlotWebAPI.WithEnvironment("AWS_SECRET_ACCESS_KEY", awsSecretAccessKey);

builder.Build().Run();
