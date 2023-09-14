using Azure.Data.Tables;

namespace producer.Interfaces
{
    public interface ITableStorageHelper
    {
        int IncrementCounter(string tableName, string partitionKey, string rowKey);
        int GetCounter(string tableName, string partitionKey, string rowKey);
    }
}
