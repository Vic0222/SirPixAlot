using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SirPixAlot.Core.EventStore.DynamoDb
{
    public class DynamoDbSetupHost(ILogger<DynamoDbSetupHost> logger, IOptions<DynamoDbEventStoreConfig> options, IAmazonDynamoDB amazonDynamoDBClient) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Setup();
        }

        public async Task Setup()
        {
            string tableName = options.Value.Table;

            logger.LogInformation("Getting table list");
            List<string> currentTables = (await amazonDynamoDBClient.ListTablesAsync()).TableNames;
            logger.LogInformation("Checking if table {table} exist", tableName);

            if (currentTables.Contains(tableName))
            {
                logger.LogInformation("Table {table} already exist", tableName);
                return;
            }
            var request = new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                  {
                    new AttributeDefinition
                    {
                      AttributeName = "GrainId",
                      AttributeType = "S"
                    },
                    new AttributeDefinition
                    {
                      AttributeName = "Version",
                      AttributeType = "N"
                    }
                  },
                KeySchema = new List<KeySchemaElement>
                  {
                    new KeySchemaElement
                    {
                      AttributeName = "GrainId",
                      // "HASH" = hash key, "RANGE" = range key.
                      KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                      AttributeName = "Version",
                      KeyType = "RANGE"
                    },
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 20,
                    WriteCapacityUnits = 20
                },
                BillingMode = BillingMode.PROVISIONED,
            };

            var response = await amazonDynamoDBClient.CreateTableAsync(request);

            logger.LogInformation("Table created with request ID: " +
              response.ResponseMetadata.RequestId);

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
