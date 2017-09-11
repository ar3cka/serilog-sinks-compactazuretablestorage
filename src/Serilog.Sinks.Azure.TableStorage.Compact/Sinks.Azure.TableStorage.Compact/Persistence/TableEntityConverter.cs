using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog.Sinks.Azure.TableStorage.Compact.SerializedLog;

namespace Serilog.Sinks.Azure.TableStorage.Compact.Persistence
{
    public class TableEntityConverter : ITableEntityConverter
    {
        /// <summary>
        /// Maximal entity size is 1MB. Out of that, we keep only 960KB (1MB - 64KB as a safety margin).
        /// Then, it should be taken into account that byte[] are Base64 encoded which represent a penalty overhead of 4/3 - hence the reduced capacity.
        /// </summary>
        private const int MAX_BYTE_CAPACITY = 15 * MAX_AZURE_TABLE_PROPERTY_SIZE_IN_KB;
        private const int MAX_AZURE_TABLE_PROPERTY_SIZE_IN_KB = 64 * 1024;

        private const string PAYLOAD_SIZE_PROPERTY_NAME = "PayloadSize";

        public DynamicTableEntity ConvertToDynamicEntity(SerializedClefLog log)
        {
            var properties = DistributeDataByProperties(log.Data);

            return new DynamicTableEntity
            {
                Properties = properties
            };
        }

        private static Dictionary<string, EntityProperty> DistributeDataByProperties(Stream data)
        {
            var properties = new Dictionary<string, EntityProperty>();
            
            var hasData = true;
            for (var i = 0; i < 15 && hasData; i++)
            {
                if (i * MAX_AZURE_TABLE_PROPERTY_SIZE_IN_KB < data.Length)
                {
                    var start = i * MAX_AZURE_TABLE_PROPERTY_SIZE_IN_KB;
                    Debug.Assert(data.Position == start);

                    var length = Math.Min(MAX_AZURE_TABLE_PROPERTY_SIZE_IN_KB, data.Length - start);
                    var buffer = new byte[length];
                    data.Read(buffer, 0, (int)length);

                    properties[GetPropertyName(i)] = new EntityProperty(buffer);
                }
                else
                {
                    hasData = false;
                }
            }

            properties[PAYLOAD_SIZE_PROPERTY_NAME] = new EntityProperty(data.Length);

            return properties;
        }

        private static string GetPropertyName(int i)
        {
            return "P" + i.ToString("D2");
        }
    }
}
