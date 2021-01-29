using Noftware.In.Faux.Shared.Data;
using Microsoft.Azure.Cosmos.Table;
using Noftware.In.Faux.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Noftware.In.Faux.Server.Azure.Entities;
using Noftware.In.Faux.Shared.Models;

namespace Noftware.In.Faux.Server.Azure
{
    /// <summary>
    /// Azure table repository.
    /// </summary>
    /// <typeparam name="TEntity">Model of type TableEntity.</typeparam>
    public abstract class TableRepository<TEntity> : ITableRepository<TEntity> where TEntity : TableEntity, new()
    {
        // Table client
        private readonly CloudTableClient _tableClient;

        // Table to be persisted to
        private readonly CloudTable _table;

        // Table partition key
        private readonly string _partitionKey;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        /// <param name="tableName">Azure table name.</param>
        /// <param name="partitionKey">Table partition key.</param>
        public TableRepository(string storageConnectionString, string tableName, string partitionKey)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
            _tableClient = account.CreateCloudTableClient();

            _table = _tableClient.GetTableReference(tableName);
            _table.CreateIfNotExists();
            _partitionKey = partitionKey;
        }

        /// <summary>
        /// Delete the Azure table.
        /// </summary>
        public async Task DeleteTableAsync()
        {
            await _table.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Create the Azure table.
        /// </summary>
        public async Task CreateTableAsync()
        {
            await _table.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Add or update the Azure table.
        /// </summary>
        /// <param name="tableEntity">Azure table.</param>
        /// <returns><see cref="Task{TableResult}"/></returns>
        public async Task<TableResult> AddOrUpdateAsync(TEntity tableEntity)
        {
            var operation = TableOperation.InsertOrReplace(tableEntity);
            var result = await _table.ExecuteAsync(operation);

            return result;
        }

        /// <summary>
        /// Get a random item based on the specified random row key and either the first item greater than rowKey, less than rowKey, or null if not found.
        /// </summary>
        /// <param name="selectColumns">Specified columns to include.</param>
        /// <returns><see cref="Task{TEntity}"/></returns>
        public async Task<TEntity> GetRandomAsync(params string[] selectColumns)
        {
            const string comparisonOperator = "gt";

            var entity = await this.GetRandomByComparisonOperatorAsync(comparisonOperator, _partitionKey, System.Guid.NewGuid().ToString(), selectColumns);

            return entity;
        }

        /// <summary>
        /// Get an item based on the row key.
        /// </summary>
        /// <param name="rowKey">The row key.</param>
        /// <returns><see cref="Task{TEntity}"/> or null, if not found.</returns>
        public async Task<TEntity> Get(string rowKey, params string[] selectColumns)
        {
            var query = new TableQuery<TEntity>()
            {
                FilterString = $"PartitionKey eq '{_partitionKey}' and RowKey eq '{rowKey}'"
            };

            // Which columns to include?
            if (selectColumns is not null && selectColumns.Length > 0)
            {
                // Include specified columns
                query.SelectColumns = selectColumns;
            }

            var queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            if (queryResult.Results.Any() == true)
            {
                return queryResult.Results.First();
            }

            return null;
        }

        /// <summary>
        /// Based on one or more filters, search for table items. PartitionKey is already included.
        /// </summary>
        /// <param name="filters">One of more filters.</param>
        /// <param name="selectColumns">Specified columns to include.</param>
        /// <returns><see cref="IAsyncEnumerable{TEntity}"/></returns>
        public async IAsyncEnumerable<TEntity> SearchAsync(IEnumerable<TableEntityFilter> filters, params string[] selectColumns)
        {
            var builder = new TableEntityFilterBuilder();
            string filter = builder.Build(filters);

            var query = new TableQuery<TEntity>()
            {
                FilterString = $"PartitionKey eq '{_partitionKey}' {filter}"
            };

            // Which columns to include?
            if (selectColumns is not null && selectColumns.Length > 0)
            {
                // Include specified columns
                query.SelectColumns = selectColumns;
            }

            TableContinuationToken token = null;
            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, token);
                foreach (var item in queryResult.Results)
                {
                    yield return item;
                }
                token = queryResult.ContinuationToken;

            } while (token is not null);
        }

        /// <summary>
        /// Get a random item based on the specified partition key and either the first item greater than rowKey, less than rowKey, or null if not found.
        /// </summary>
        /// <param name="comparisonOperator">OData comparison operators, i.e. gt, lt, eq, etc.</param>
        /// <param name="partitionKey">Partition key.</param>
        /// <param name="rowKey">Row key used in the greater than or less than comparison.</param>
        /// <param name="selectColumns">Specified columns to include.</param>
        /// <returns><see cref="Task{TEntity}"/></returns>
        private async Task<TEntity> GetRandomByComparisonOperatorAsync(string comparisonOperator, string partitionKey, string rowKey, params string[] selectColumns)
        {
            var query = new TableQuery<TEntity>()
            {
                FilterString = $"PartitionKey eq '{partitionKey}' and RowKey {comparisonOperator} '{rowKey}'",
                TakeCount = 1
            };

            // Which columns to include?
            if (selectColumns is not null && selectColumns.Length > 0)
            {
                // Include specified columns
                query.SelectColumns = selectColumns;
            }

            var queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            if (queryResult.Results.Any() == true)
            {
                return queryResult.Results.First();
            }

            return null;
        }
    }
}
