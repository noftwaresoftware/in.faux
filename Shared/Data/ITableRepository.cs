using Microsoft.Azure.Cosmos.Table;
using Noftware.In.Faux.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Shared.Data
{
    /// <summary>
    /// Azure table repository interface.
    /// </summary>
    /// <typeparam name="TEntity">Model of type TableEntity.</typeparam>
    public interface ITableRepository<TEntity> where TEntity : TableEntity, new()
    {
        /// <summary>
        /// Delete the Azure table.
        /// </summary>
        Task DeleteTableAsync();

        /// <summary>
        /// Create the Azure table.
        /// </summary>
        Task CreateTableAsync();

        /// <summary>
        /// Add or update the Azure table.
        /// </summary>
        /// <param name="tableEntity">Azure table.</param>
        /// <returns><see cref="Task{TableResult}"/></returns>
        Task<TableResult> AddOrUpdateAsync(TEntity tableEntity);

        /// <summary>
        /// Get a random item based on the partition key and either the first item greater than rowKey, less than rowKey, or null if not found.
        /// </summary>
        /// <param name="selectColumns">Specified columns to include.</param>
        /// <returns><see cref="Task{TEntity}"/></returns>
        Task<TEntity> GetRandomAsync(params string[] selectColumns);

        /// <summary>
        /// Based on one or more filters, search for table items.
        /// </summary>
        /// <param name="filters">One of more filters.</param>
        /// <param name="selectColumns">Specified columns to include.</param>
        /// <returns><see cref="IAsyncEnumerable{TEntity}"/></returns>
        IAsyncEnumerable<TEntity> SearchAsync(IEnumerable<TableEntityFilter> filters, params string[] selectColumns);

        /// <summary>
        /// Get an item based on the row key.
        /// </summary>
        /// <param name="rowKey">The row key.</param>
        /// <returns><see cref="Task{TEntity}"/> or null, if not found.</returns>
        Task<TEntity> Get(string rowKey, params string[] selectColumns);
    }
}