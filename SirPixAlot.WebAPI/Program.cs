using Amazon.DynamoDBv2;
using Libsql.Client;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;
using Orleans.Storage;
using SirPixAlot.Core.EventStore;
using SirPixAlot.Core.EventStore.Libsql;
using SirPixAlot.Core.Infrastructure;
using SirPixAlot.Core.Metrics;
using Orleans.Clustering.DynamoDB;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.IsDevelopment();
// Add orleans
builder.Host.UseOrleans(static async siloBuilder =>
{
    var awsAccesskey = siloBuilder.Configuration["AWS_ACCESS_KEY_ID"];
    var awsSecretKey = siloBuilder.Configuration["AWS_SECRET_ACCESS_KEY"];
    if (!string.IsNullOrEmpty(siloBuilder.Configuration["FLY_PRIVATE_IP"])) //this means we are running in fly.io
    {
        siloBuilder.Configure<EndpointOptions>(options =>
        {
            options.AdvertisedIPAddress = IPAddress.Parse(siloBuilder.Configuration["FLY_PRIVATE_IP"]);
        });
    }
    

    if (!string.IsNullOrEmpty(awsAccesskey) && !string.IsNullOrEmpty(awsSecretKey))
    {
        //siloBuilder.UseKubernetesHosting();
        siloBuilder.UseDynamoDBClustering(options => {
            options.AccessKey = siloBuilder.Configuration["AWS_ACCESS_KEY_ID"];
            options.SecretKey = siloBuilder.Configuration["AWS_SECRET_ACCESS_KEY"];
            options.Service = siloBuilder.Configuration["AWS_DEFAULT_REGION"];
            
            if (!string.IsNullOrEmpty(siloBuilder.Configuration["DynamoDBClusteringOptions:TableName"]))
            {
                options.TableName = siloBuilder.Configuration["DynamoDBClusteringOptions:TableName"];
            }
        });
    }
    else
    {
        siloBuilder.UseLocalhostClustering();
    }

    siloBuilder.AddCustomStorageBasedLogConsistencyProviderAsDefault();

   

});

//siloBuilder.Services.AddSingleton<IAmazonDynamoDB>(svc => new AmazonDynamoDBClient());
//siloBuilder.Services.AddSingleton<IEventStorage, DynamoDbEventStorage>();
//siloBuilder.Services.Configure<DynamoDbEventStoreConfig>(siloBuilder.Configuration.GetSection("EventStoreConfig"));
var libsqlDbClient = await DatabaseClient.Create(option =>
{
    builder.Configuration.GetSection("LibsqlDatabaseClientOptions").Bind(option);
    if (string.IsNullOrEmpty(option.ReplicaPath)) //the turso client throws an error if replica path is empty instead of null
    {
        option.ReplicaPath = null;
    }
    //option.UseHttps = false;
});
builder.Services.AddSingleton<ISqliteConnectionProvider>(svc
    => new SqliteConnectionProvider(builder.Configuration.GetConnectionString("Sqlite") ?? string.Empty));

builder.Services.AddSingleton(libsqlDbClient);
builder.Services.AddSingleton<IEventStorage, LibqsqlEventStorage>();

builder.Services.AddSingleton<SirPixAlotMetrics>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});


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
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
