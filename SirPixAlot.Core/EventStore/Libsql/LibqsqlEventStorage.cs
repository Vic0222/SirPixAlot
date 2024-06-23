﻿using Libsql.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lql = Libsql.Client;

namespace SirPixAlot.Core.EventStore.Libsql
{
    public class LibqsqlEventStorage(IDatabaseClient databaseClient, ILogger<LibqsqlEventStorage> logger) : IEventStorage
    {
        public async Task<List<Event>> ReadEvents(string grainId, int version)
        {
            var result = await databaseClient.Execute($"SELECT `grain_id`, `version`, cast(  `global_position` as text), `event_type`, `data` FROM  `pixel_grains` where `grain_id` = '{grainId}'");
            return result.Rows.Select(ToEvent).ToList();
        }

        public async Task<bool> SaveEvents(IReadOnlyCollection<Event> events)
        {
            try
            {


                //throw new NotImplementedException();
                if (events.Count == 0)
                {
                    return false;
                }

                //validate there's no version conflict
                var graindId = events.FirstOrDefault()?.GrainId ?? string.Empty;
                var versions = events.Select(e => e.Version).ToHashSet();
                var countResult = await databaseClient.Execute($"SELECT COUNT(*) FROM `pixel_grains` where `grain_id` = '{graindId}' and `version` in ({string.Join(',', versions)}) ");
                var count = ToCount(countResult.Rows.First());
                if (count > 0)
                {
                    return false;
                }

                //WARNING:
                //Event data contains some user input that can be used for sql injection.
                //While it's already validated it would be nice to execute queries using parameters.
                //But sadly it's not yet supported by the current libsql client.
                foreach (var e in events)
                {
                    var result = await databaseClient.Execute($"INSERT INTO `pixel_grains` (`grain_id`, `version`, `global_position`, `event_type`, `data`) " +
                    $"VALUES ('{e.GrainId}', {e.Version}, '{e.GlobalPosition}', '{e.EventType}', '{e.Data}')");

                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving events");
                throw;
            }
        }

        private Event ToEvent(IEnumerable<Value> row)
        {
            var rowArray = row.ToArray();

            if (
                rowArray[0] is Text { Value: var grainId } &&
                rowArray[1] is Integer { Value: var version } &&
                rowArray[2] is Text { Value: var globalPosition } &&
                rowArray[3] is Text { Value: var event_type } &&
                rowArray[4] is Text { Value: var data })
            {
                long.TryParse(globalPosition, out var globalPositionLong);  
                return new Event
                {
                    GrainId = grainId ?? string.Empty,
                    Version = version,
                    GlobalPosition = globalPositionLong,
                    EventType = event_type ?? string.Empty,
                    Data = data ?? string.Empty,
                };
            }

            throw new ArgumentException();
        }

        private int ToCount(IEnumerable<Value> row)
        {
            var rowArray = row.ToArray();

            if ( rowArray[0] is Integer { Value: var count })
            {
                return count;
            }

            throw new ArgumentException();
        }

    }
}