using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var grainStorage = builder.AddParameter("EventStoreConfig-SirPixAlotGrainStorage");
var testSecret = builder.AddParameter("testSecret",secret: true);

var sirPixAlotWebAPI =  builder.AddProject<Projects.SirPixAlot_WebAPI>("SirPixAlotWebAPI");
sirPixAlotWebAPI.WithEnvironment("EventStoreConfig__SirPixAlotGrainStorage", grainStorage);
sirPixAlotWebAPI.WithEnvironment("testSecret", testSecret);

builder.Build().Run();
