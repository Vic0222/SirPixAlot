using Libsql.Client;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.EventStore.Libsql
{
    public class LibsqlSetupHost(IDatabaseClient databaseClient) : BackgroundService
    {
        private readonly PeriodicTimer _timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            await databaseClient.Execute("CREATE TABLE IF NOT EXISTS `pixel_grains` (`grain_id` TEXT NOT NULL, `version` INTEGER NOT NULL, `global_position` BIGINT NOT NULL, `event_type` TEXT NOT NULL, `data` TEXT NOT NULL, PRIMARY KEY ( `grain_id`, `version`))");

            await base.StartAsync(cancellationToken);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //move this to a stateless grain
            //so each silo has it's own copy
            while (await _timer.WaitForNextTickAsync(stoppingToken))
            {
                await databaseClient.Sync();
            }
        }
    }
}
