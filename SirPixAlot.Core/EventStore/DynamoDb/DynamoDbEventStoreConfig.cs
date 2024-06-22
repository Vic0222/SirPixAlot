using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.EventStore.DynamoDb
{
    public class DynamoDbEventStoreConfig
    {
        public string Table { get; set; } = string.Empty;
    }
}
