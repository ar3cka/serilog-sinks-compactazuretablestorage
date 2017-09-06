using System;
using Microsoft.WindowsAzure.Storage;
using Serilog.Configuration;
using Serilog.Sinks.Azure.TableStorage.Compact;

namespace Serilog
{
    public static class LoggerConfigurationForAzureTableStorageCompactExtensions
    {
        public static LoggerConfiguration AzureTableStorageWithCompactedRowFormat(
            this LoggerSinkConfiguration configuration,
            CloudStorageAccount cloudStorageAccount,
            string tableName)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (cloudStorageAccount == null) throw new ArgumentNullException(nameof(cloudStorageAccount));
            if (tableName == null) throw new ArgumentNullException(nameof(tableName));

            return configuration.Sink(
                new AzureTableStorageWithCompactedRowFormatSink(
                    cloudStorageAccount: cloudStorageAccount,
                    tableName: tableName,
                    batchSizeLimit: 100,
                    period: TimeSpan.FromSeconds(5)));
        }
    }
}
