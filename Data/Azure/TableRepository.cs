using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Noftware.In.Faux.Core.Data;
using Noftware.In.Faux.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Data.Azure
{
    /// <summary>
    /// Azure table repository.
    /// </summary>
    /// <typeparam name="TEntity">Model of type TableEntity.</typeparam>
    public abstract class TableRepository<TEntity> : ITableRepository<TEntity> where TEntity : BaseTableEntity, new()
    {
        // Table client
        private readonly TableClient _tableClient;

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
            var tableServiceClient = new TableServiceClient(storageConnectionString);

            _tableName = tableName;

            _tableClient = tableServiceClient.GetTableClient(_tableName);
            _tableClient.CreateIfNotExists();
            _partitionKey = partitionKey;
        }

        /// <summary>
        /// Delete the Azure table.
        /// </summary>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> DeleteTableAsync()
        {
            Response response = null;
            bool success = false;

            try
            {
                response = await _tableClient.DeleteAsync();

                // Final status notification
                success = !response.IsError;
                if (success == true)
                {
                    OnStatusUpdate($"{_tableName} table was successfully deleted.", OperationStatus.Success);
                }
                else
                {
                    OnStatusUpdate($"{_tableName} table was not successfully deleted (Status: {response.Status}).", OperationStatus.Error);
                }
            }
            catch (Exception ex)
            {
                OnStatusUpdate($"An error occurred deleting table {_tableName}. {ex}", OperationStatus.Error);
            }

            response?.Dispose();

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
                    Response<TableItem> response = await _tableClient.CreateIfNotExistsAsync();
                    count = MaxTries + 1;
                    success = true;
                }
                catch (RequestFailedException reqEx)
                {
                    if (reqEx.ErrorCode == TableErrorCode.TableBeingDeleted)
                    {
                        OnStatusUpdate($"{count + 1}: The {_tableName} table is currently being created. Please wait.", OperationStatus.Warning);
                        await Task.Delay(1000); // The table is currently being deleted. Try again until it works.
                    }
                    else
                    {
                        OnStatusUpdate($"A storage error occurred creating table {_tableName}. {reqEx}", OperationStatus.Error);
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
                OnStatusUpdate($"nameof{tableEntity} was null. Unable to persist.", OperationStatus.Error);
                return false;
            }
            try
            {
                var response = await _tableClient.UpsertEntityAsync(tableEntity);
                if (response.IsError == true)
                {
                    OnStatusUpdate($"An error occurred persisting to {_tableName}. {response.ReasonPhrase} (Status: {response.Status})", OperationStatus.Error);
                }
                else
                {
                    OnStatusUpdate($"Persisted row key '{tableEntity.RowKey}' to table {_tableName}.", OperationStatus.Success);
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnStatusUpdate($"An error occurred persisting row key '{tableEntity.RowKey}' to table {_tableName}. {ex}", OperationStatus.Error);
            }

            return false;
        }

        /// <summary>
        /// Get an item based on the row key.
        /// </summary>
        /// <param name="rowKey">The row key.</param>
        /// <param name="selectColumns">Columns to select.</param>
        /// <returns><see cref="BaseTableEntity"/> or null, if not found.</returns>
        public async Task<TEntity> GetAsync(string rowKey, params string[] selectColumns)
        {
            // Which columns to include?
            Response<TEntity> response;
            if (selectColumns != null && selectColumns.Length > 0)
            {
                // Include specified columns
                response = await _tableClient.GetEntityAsync<TEntity>(_partitionKey, rowKey, selectColumns);
            }
            else
            {
                response = await _tableClient.GetEntityAsync<TEntity>(_partitionKey, rowKey);
            }

            if (response is not null)
            {
                OnStatusUpdate($"Successfully obtained '{rowKey}' from table {_tableName}.", OperationStatus.Success);
                return response.Value;
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

            string filterString = $"PartitionKey eq '{_partitionKey}' {filter}";

            // Which columns to include?
            AsyncPageable<TEntity> results;
            if (selectColumns != null && selectColumns.Length > 0)
            {
                // Include specified columns
                results = _tableClient.QueryAsync<TEntity>(filter: filterString, select: selectColumns);
            }
            else
            {
                results = _tableClient.QueryAsync<TEntity>(filter: filterString);
            }

            await foreach (var item in results)
            {
                yield return item;
            }
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
