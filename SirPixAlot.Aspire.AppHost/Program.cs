var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SirPixAlot_WebAPI>("SirPixAlotWebAPI");
builder.Build().Run();
