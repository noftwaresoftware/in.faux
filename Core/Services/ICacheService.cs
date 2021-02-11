using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Core.Services
{
    /// <summary>
    /// Cached items.
    /// </summary>
    /// <typeparam name="TEntity">The entity to cache.</typeparam>
    /// <typeparam name="TKey">The type of unique identifier of <see cref="TEntity"/>.</typeparam>
    public interface ICacheService<TEntity, TKey>
    {
        /// <summary>
        /// Clear all cached items.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task ClearAsync();

        /// <summary>
        /// Add or update a cached item.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        Task AddOrUpdateAsync(TEntity entity);

        /// <summary>
        /// Check for item existence by key.
        /// </summary>
        /// <param name="key">Unique identifier.</param>
        /// <returns><see cref="Task{bool}"/></returns>
        Task<bool> ExistsAsync(TKey key);

        /// <summary>
        /// Get item by key.
        /// </summary>
        /// <param name="key">Unique identifier.</param>
        /// <returns><see cref="Task{TEntity}"/></returns>
        Task<TEntity> GetAsync(TKey key);

        /// <summary>
        /// Get random item.
        /// </summary>
        /// <returns><see cref="Task{TEntity}"/></returns>
        Task<TEntity> GetRandomAsync();

        /// <summary>
        /// Remove item by key.
        /// </summary>
        /// <param name="key">Unique identifier.</param>
        /// <returns><see cref="Task"/></returns>
        Task RemoveAsync(TKey key);
    }
}
