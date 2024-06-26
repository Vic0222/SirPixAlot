using Libsql.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Configuration.Overrides;
using Orleans.Runtime;
using Orleans.Storage;
using SirPixAlot.Core.EventStore;
using SirPixAlot.Core.EventStore.Libsql;
using SirPixAlot.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Core;

namespace SirPixAlot.Core.StateStore.Libsql
{
    public class LibsqlGrainStorage(string storageName, IOptions<ClusterOptions> clusterOptions, IOptions<LibsqlGrainStorageOptions> options, IDatabaseClient databaseClient, ISqliteConnectionProvider sqliteConnectionProvider, ILogger<LibsqlGrainStorage> logger) : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>

    {

        public void Participate(ISiloLifecycle lifecycle)
        {
            lifecycle.Subscribe(
                observerName: OptionFormattingUtilities.Name<LibsqlGrainStorage>(_=storageName),
                stage: ServiceLifecycleStage.ApplicationServices,
                onStart: (ct) =>
                {
                    return databaseClient.Execute("CREATE TABLE IF NOT EXISTS `grain_states` (`GrainId` TEXT NOT NULL, `Name` TEXT NOT NULL, `Version` INTEGER NOT NULL,  `data` BLOB NOT NULL, PRIMARY KEY ( `GrainId`, `Name`))");
                });

        }

        public Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            return databaseClient.Execute($"DELETE FROM `grain_states` where `GrainId` = '{grainId}' and `Name` = '{stateName}'");
        }


        public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            var result = await databaseClient.Execute($"SELECT * FROM `grain_states` where `GrainId` = '{grainId}' and `Name` = '{stateName}'  LIMIT 1");

            if (result.Rows.Any())
            {
                var dbState = ToState(result.Rows.First());
                grainState.State = options.Value.GrainStorageSerializer.Deserialize<T>(new BinaryData(dbState.Data));
                grainState.ETag = dbState.Version.ToString();
            }
        }

        public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
        {
            
            int.TryParse(grainState.ETag, out int currentVersion);
            if (currentVersion > 0)
            {
                var result = await databaseClient.Execute($"SELECT * FROM `grain_states` where `grain_id` = '{grainId}' LIMIT 1");

                if (result.Rows.Any())
                {
                    var dbState = ToState(result.Rows.First());
                    if (dbState.Version != currentVersion)
                    {
                        throw new InconsistentStateException($"""
                        Version conflict (WriteState): ServiceId={clusterOptions.Value.ServiceId}
                        ProviderName={storageName} GrainType={typeof(T)}
                        GrainReference={grainId}.
                        """);

                    }
                }
            }

            BinaryData storedData = options.Value.GrainStorageSerializer.Serialize(grainState.State);

            await databaseClient.Execute($"INSERT INTO `grain_states` (`GrainId`, `Name`, `Version`, `Data`) " +
                    $"VALUES ('{grainId}', {stateName}, {++currentVersion}, '{storedData.ToArray()}')");

            grainState.ETag = currentVersion.ToString();
        }

        private GrainState ToState(IEnumerable<Value> row)
        {
            var rowArray = row.ToArray();

            if (
                rowArray[0] is Text { Value: var grainId } &&
                rowArray[1] is Integer { Value: var version } &&
                rowArray[2] is Text { Value: var name } &&
                rowArray[3] is Blob { Value: var data } 
                )
            {
                return new GrainState
                {
                    GrainId = grainId ?? string.Empty,
                    Version = version,
                    Name = name,
                    Data = data
                };
            }

            throw new ArgumentException();
        }
    }

    public sealed class LibsqlGrainStorageOptions : IStorageProviderSerializerOptions
    {

        public required IGrainStorageSerializer GrainStorageSerializer { get; set; }
    }

    internal static class LibsqlGrainStorageFactory
    {
        internal static IGrainStorage Create(
            IServiceProvider services, string name)
        {
            var optionsMonitor =
                services.GetRequiredService<IOptionsMonitor<LibsqlGrainStorageOptions>>();

            return ActivatorUtilities.CreateInstance<LibsqlGrainStorage>(
                services,
                name,
                optionsMonitor.Get(name),
                services.GetProviderClusterOptions(name));
        }

    }

    public static class LibsqlSiloBuilderExtensions
    {
        public static ISiloBuilder AddLibsqlGrainStorage(
            this ISiloBuilder builder,
            string providerName,
            Action<LibsqlGrainStorageOptions> options) =>
            builder.ConfigureServices(
                services => services.AddLibsqlStorage(
                    providerName, options));

        public static IServiceCollection AddLibsqlStorage(
            this IServiceCollection services,
            string providerName,
            Action<LibsqlGrainStorageOptions> options)
        {
            services.AddOptions<LibsqlGrainStorageOptions>(providerName)
                .Configure(options);

            services.AddTransient<
                IPostConfigureOptions<LibsqlGrainStorageOptions>,
                DefaultStorageProviderSerializerOptionsConfigurator<LibsqlGrainStorageOptions>>();

            return services.AddKeyedSingleton(providerName, LibsqlGrainStorageFactory.Create)
                .AddKeyedSingleton(providerName,
                    (p, n) =>
                        (ILifecycleParticipant<ISiloLifecycle>)p.GetRequiredKeyedService<IGrainStorage>(n));
        }
    }

}
