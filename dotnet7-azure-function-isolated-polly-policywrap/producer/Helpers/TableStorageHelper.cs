using Azure.Data.Tables;
using Azure;
using producer.Interfaces;

namespace producer.Helpers
{
    public class TableStorageHelper : ITableStorageHelper
    {
        private readonly TableServiceClient _serviceClient;

        public TableStorageHelper(string connectionString)
        {
            _serviceClient = new TableServiceClient(connectionString);
        }

        public int IncrementCounter(string tableName, string partitionKey, string rowKey)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);

            var entity = GetEntity(tableName, partitionKey, rowKey);

            if(entity != null)
            {
                var currentCounter = int.Parse(entity[Constants.Counter].ToString() ?? "0");
                var newCounter = currentCounter + 1;
                entity[Constants.Counter] = newCounter.ToString();
                tableClient.UpdateEntity(entity, ETag.All);
                return newCounter;
            }

            throw new Exception($"Entity '{rowKey}' not found in table '{tableName}'.");
        }

        public int GetCounter(string tableName, string partitionKey, string rowKey)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);

            var entity = GetEntity(tableName, partitionKey, rowKey);

            if (entity != null)
            {
                var currentCounter = int.Parse(entity[Constants.Counter].ToString() ?? "0");
                return currentCounter;
            }

            throw new Exception($"Entity '{rowKey}' not found in table '{tableName}'.");
        }

        private TableEntity? GetEntity(string tableName, string partitionKey, string rowKey)
        {
            _serviceClient.CreateTableIfNotExists(tableName);
            var tableClient = _serviceClient.GetTableClient(tableName);

            try
            {
                return tableClient.GetEntity<TableEntity>(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }
    }
}
