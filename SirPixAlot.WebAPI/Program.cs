using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;
using Orleans.Storage;
using SirPixAlot.Core.EventStore;

var builder = WebApplication.CreateBuilder(args);

// Add orleans
builder.Host.UseOrleans(static siloBuilder =>
{
    
    siloBuilder.UseLocalhostClustering();
    

    siloBuilder.AddCustomStorageBasedLogConsistencyProviderAsDefault();

    siloBuilder.Services.AddSingleton<IAmazonDynamoDB>(svc => new AmazonDynamoDBClient());
    siloBuilder.Services.AddSingleton<IEventStorage, EventStorage>();
    siloBuilder.Services.Configure<EventStoreConfig>(siloBuilder.Configuration.GetSection("EventStoreConfig"));

});

// Add services to the container.

builder.Services.AddHostedService<SetupHost>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
