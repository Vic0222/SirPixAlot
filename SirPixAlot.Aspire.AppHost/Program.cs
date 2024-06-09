using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var sirPixAlotWebAPI =  builder.AddProject<Projects.SirPixAlot_WebAPI>("SirPixAlotWebAPI");
if (builder.Environment.EnvironmentName == "Production")
{
    sirPixAlotWebAPI.WithEnvironment("EventStoreConfig__SirPixAlotGrainStorageDev", "SirPixAlotGrainStorage");
}
builder.Build().Run();
