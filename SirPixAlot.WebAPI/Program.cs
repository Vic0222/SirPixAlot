using Amazon.DynamoDBv2;
using Libsql.Client;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;
using Orleans.Storage;
using SirPixAlot.Core.EventStore;
using SirPixAlot.Core.EventStore.DynamoDb;
using SirPixAlot.Core.EventStore.Libsql;
using SirPixAlot.Core.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add orleans
builder.Host.UseOrleans(static async siloBuilder =>
{
    
    siloBuilder.UseLocalhostClustering();
    

    siloBuilder.AddCustomStorageBasedLogConsistencyProviderAsDefault();

    //siloBuilder.Services.AddSingleton<IAmazonDynamoDB>(svc => new AmazonDynamoDBClient());
    //siloBuilder.Services.AddSingleton<IEventStorage, DynamoDbEventStorage>();
    //siloBuilder.Services.Configure<DynamoDbEventStoreConfig>(siloBuilder.Configuration.GetSection("EventStoreConfig"));
    var libsqlDbClient = await DatabaseClient.Create(option =>
    {
        siloBuilder.Configuration.GetSection("LibsqlDatabaseClientOptions").Bind(option);
        //option.UseHttps = false;
    });
    siloBuilder.Services.AddSingleton(libsqlDbClient);
    siloBuilder.Services.AddSingleton<IEventStorage, LibqsqlEventStorage>();

    siloBuilder.Services.AddSingleton<SirPixAlotMetrics>();

});

// Add services to the container.

//builder.Services.AddHostedService<DynamoDbSetupHost>();
builder.Services.AddHostedService<LibsqlSetupHost>();

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

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
