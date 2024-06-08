using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Transform;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.EventStore
{
    public interface IEventStorage
    {
        Task<List<Event>> ReadEvents(string grainId, int version);
        Task<bool> SaveEvents(IReadOnlyCollection<Event> events);
    }

    public class EventStorage(ILogger<EventStorage> logger, IOptions<EventStoreConfig> options, IAmazonDynamoDB amazonDynamoDBClient) : IEventStorage
    {
        

        public async Task<bool> SaveEvents(IReadOnlyCollection<Event> events)
        {
            string tableName = options.Value.Table;
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Put entries {TableName} table", tableName);
            }

            if (events.Count == 0)
                return false;

            try
            {
                foreach (Event @event in events)
                {
                    try
                    {
                        var item = new Dictionary<string, AttributeValue>
                    {
                        { "GrainId", new AttributeValue(@event.GrainId) },
                        { "Version", new AttributeValue() { N = @event.Version.ToString() } },
                        { "GlobalPosition", new AttributeValue() { N = @event.GlobalPosition.ToString() } },
                        { "EventType", new AttributeValue(@event.EventType) },
                        { "Data", new AttributeValue(@event.Data) }
                    };
                        var request = new PutItemRequest
                        {
                            TableName = tableName,
                            ConditionExpression = "attribute_not_exists(Version)",
                            Item = item
                        };
                        await amazonDynamoDBClient.PutItemAsync(request);
                    }
                    catch (ConditionalCheckFailedException ex)
                    {
                        logger.LogWarning(
                            (int)ErrorCode.StorageProviderBase,
                            ex,
                            "Version conflict error inserting entries to table {TableName} {GraindId} {Version}.",
                            tableName, @event.GrainId, @event.Version);

                        return false;
                    }
                }

            }
            catch (Exception exc)
            {
                logger.LogWarning(
                    (int)ErrorCode.StorageProviderBase,
                    exc,
                    "Intermediate error bulk inserting entries to table {TableName}.",
                    tableName);
                throw;
            }

            return true;
        }

        public async Task<List<Event>> ReadEvents(string grainId, int version)
        {

            string tableName = options.Value.Table;
            if (logger.IsEnabled(LogLevel.Trace))
            {
                logger.LogTrace("Read events {TableName} table", tableName);
            }
            try
            {
                var query = new QueryRequest()
                {
                    TableName = tableName,
                    KeyConditionExpression = "GrainId = :grainId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {
                            ":grainId" , new AttributeValue
                            {
                                S = grainId
                            }
                        } 
                    }
                };
                var response = await amazonDynamoDBClient.QueryAsync(query);
                var events = new List<Event>();
                foreach (var item in response.Items)
                {
                    var @event = new Event()
                    {
                        GrainId = GetString(item, "GrainId"),
                        Version = GetNumber(item, "Version"),
                        GlobalPosition = GetNumber(item, "GlobalPosition"),
                        EventType = GetString(item, "EventType"),
                        Data = GetString(item, "Data"),
                    };
                    events.Add(@event);
                }
                return events.OrderBy(e => e.Version).ToList();
            }
            catch (Exception)
            {
                if (logger.IsEnabled(LogLevel.Debug)) logger.LogDebug("Unable to find table entry for grainId = {grainId}", grainId);
                throw;
            }
        }

        private string GetString(Dictionary<string, AttributeValue> item, string key)
        {
            if (item.TryGetValue(key, out AttributeValue? attributeValue))
            {
                return attributeValue?.S ?? string.Empty;
            }
            return string.Empty;
        }

        private int GetNumber(Dictionary<string, AttributeValue> item, string key)
        {
            if (item.TryGetValue(key, out AttributeValue? attributeValue))
            {
                var n = attributeValue?.N ?? "0";
                if (int.TryParse(n, out int number))
                {
                    return number;
                }
            }
            return 0;
        }
    }
}
