var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SirPixAlot_WebAPI>("WebAPI");
builder.Build().Run();
