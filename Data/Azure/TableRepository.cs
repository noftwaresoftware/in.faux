using Noftware.In.Faux.Core.Data;
using Microsoft.Azure.Cosmos.Table;
using Noftware.In.Faux.Core.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Noftware.In.Faux.Data.Azure.Entities;
using Noftware.In.Faux.Core.Models;
using Microsoft.Azure.Cosmos.Table.Protocol;

namespace Noftware.In.Faux.Data.Azure
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

        // Table name
        private readonly string _tableName;

        // Table partition key
        private readonly string _partitionKey;

        /// <summary>
        /// Event handler to notify caller of status.
        /// </summary>
        public event EventHandler<TableRepositoryEventArgs> StatusUpdate;

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

            _tableName = tableName;

            _table = _tableClient.GetTableReference(tableName);
            _table.CreateIfNotExists();
            _partitionKey = partitionKey;
        }

        /// <summary>
        /// Delete the Azure table.
        /// </summary>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> DeleteTableAsync()
        {
            bool success = false;

            try
            {
                success = await _table.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                OnStatusUpdate($"An error occurred deleting table {_tableName}. {ex}", OperationStatus.Error);
            }

            // Final status notification
            if (success == true)
            {
                OnStatusUpdate($"{_tableName} table was successfully deleted.", OperationStatus.Success);
            }
            else
            {
                OnStatusUpdate($"{_tableName} table was not successfully deleted.", OperationStatus.Error);
            }

            return success;
        }

        /// <summary>
        /// Create the Azure table.
        /// </summary>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> CreateTableAsync()
        {
            bool success = false;

            // https://stackoverflow.com/questions/15508517/the-correct-way-to-delete-and-recreate-a-windows-azure-storage-table-error-409
            const int MaxTries = 60;
            int count = 0;
            do
            {
                try
                {
                    await _table.CreateIfNotExistsAsync();
                    count = MaxTries + 1;
                    success = true;
                }
                catch (StorageException stgEx)
                {
                    if ((stgEx.RequestInformation.HttpStatusCode == 409) && (stgEx.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted)))
                    {
                        OnStatusUpdate($"{count + 1}: The {_tableName} table is currently being created. Please wait.", OperationStatus.Warning);
                        await Task.Delay(1000); // The table is currently being deleted. Try again until it works.
                    }
                    else
                    {
                        OnStatusUpdate($"A storage error occurred creating table {_tableName}. {stgEx}", OperationStatus.Error);
                    }
                }
                catch (Exception ex)
                {
                    // Catch-all
                    OnStatusUpdate($"An error occurred creating table {_tableName}. {ex}", OperationStatus.Error);
                }

                count++;

            } while (count < MaxTries);

            // Final status notification
            if (success == true)
            {
                OnStatusUpdate($"{_tableName} table was successfully created.", OperationStatus.Success);
            }
            else
            {
                OnStatusUpdate($"{_tableName} table was not successfully created. Retry attempts: {count}.", OperationStatus.Error);
            }

            return success;
        }

        /// <summary>
        /// Add or update the Azure table.
        /// </summary>
        /// <param name="tableEntity">Azure table.</param>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> AddOrUpdateAsync(TEntity tableEntity)
        {
            if (tableEntity is null)
            {
                OnStatusUpdate($"nameof{tableEntity} was null. Unable to persist to table {_tableName}.", OperationStatus.Error);
                return false;
            }

            var operation = TableOperation.InsertOrReplace(tableEntity);
            try
            {
                var result = await _table.ExecuteAsync(operation);
                OnStatusUpdate($"Persisted row key '{tableEntity.RowKey}' to table {_tableName}.", OperationStatus.Success);
                return true;
            }
            catch (Exception ex)
            {
                OnStatusUpdate($"An error occurred persisting row key '{tableEntity.RowKey}' to table {_tableName}. {ex}", OperationStatus.Error);
                return false;
            }
        }

        /// <summary>
        /// Get an item based on the row key.
        /// </summary>
        /// <param name="rowKey">The row key.</param>
        /// <param name="selectColumns">Columns to select.</param>
        /// <returns><see cref="TableEntity"/> or null, if not found.</returns>
        public async Task<TEntity> GetAsync(string rowKey, params string[] selectColumns)
        {
            var query = new TableQuery<TEntity>()
            {
                FilterString = $"PartitionKey eq '{_partitionKey}' and RowKey eq '{rowKey}'"
            };

            // Which columns to include?
            if (selectColumns != null && selectColumns.Length > 0)
            {
                // Include specified columns
                query.SelectColumns = selectColumns;
            }

            var queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            if (queryResult.Results.Any() == true)
            {
                OnStatusUpdate($"Successfully obtained {rowKey} from table {_tableName}.", OperationStatus.Success);
                return queryResult.Results.First();
            }

            OnStatusUpdate($"{rowKey} not found in table {_tableName}.", OperationStatus.Error);
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
            if (selectColumns != null && selectColumns.Length > 0)
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

            } while (token != null);
        }

        /// <summary>
        /// Event handler to notify caller of status.
        /// </summary>
        /// <param name="message">Status message.</param>
        /// <param name="operationStatus">Status of the operation.</param>
        private void OnStatusUpdate(string message, OperationStatus operationStatus)
        {
            this.StatusUpdate?.Invoke(this, new TableRepositoryEventArgs(message, operationStatus));
        }
    }
}
