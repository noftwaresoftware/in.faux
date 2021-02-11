using Noftware.In.Faux.Core.Models;
using Noftware.In.Faux.Core.Services;
using Noftware.In.Faux.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Data.Services
{
    /// <summary>
    /// Quote caching service.
    /// </summary>
    public class QuoteCacheService : ICacheService<Quote, string>
    {
        // Cached list
        private readonly List<Quote> _cachedQuotes = new List<Quote>();

        /// <summary>
        /// Add or update a cached quote.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public Task AddOrUpdateAsync(Quote entity)
        {
            var existingQuote = _cachedQuotes.FirstOrDefault(f => f.Key == entity.Key);
            if (existingQuote is null)
            {
                // Add it
                _cachedQuotes.Add(existingQuote);
            }
            else
            {
                // Update existing item
                existingQuote.Base64Image = entity.Base64Image;
                existingQuote.Description = entity.Description;
                existingQuote.FileName = entity.FileName;
                existingQuote.Keywords = entity.Keywords;
                existingQuote.Text = entity.Text;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Clear all cached quotes.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public Task ClearAsync()
        {
            _cachedQuotes.Clear();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Check for quote existence by key.
        /// </summary>
        /// <param name="key">Unique identifier.</param>
        /// <returns><see cref="Task{bool}"/></returns>
        public Task<bool> ExistsAsync(string key)
        {
            bool exists = _cachedQuotes.Any(f => f.Key == key);
            return Task.FromResult(exists);
        }

        /// <summary>
        /// Get quote by key.
        /// </summary>
        /// <param name="key">Unique identifier.</param>
        /// <returns><see cref="Task{Quote}"/></returns>
        public Task<Quote> GetAsync(string key)
        {
            var quote = _cachedQuotes.FirstOrDefault(f => f.Key == key);
            return Task.FromResult(quote);
        }

        /// <summary>
        /// Get random quote.
        /// </summary>
        /// <returns><see cref="Task{Quote}"/></returns>
        public Task<Quote> GetRandomAsync()
        {
            if (_cachedQuotes.Count > 0)
            {
                // Return random quote
                int index = _cachedQuotes.GetRandomIndex();
                var quote = _cachedQuotes[index];
                return Task.FromResult(quote);
            }
            else
            {
                // No quotes. Return null.
                return Task.FromResult<Quote>(null);
            }
        }

        /// <summary>
        /// Remove quote by key.
        /// </summary>
        /// <param name="key">Unique identifier.</param>
        /// <returns><see cref="Task"/></returns>
        public Task RemoveAsync(string key)
        {
            _cachedQuotes.RemoveAll(r => r.Key == key);
            return Task.CompletedTask;
        }
    }
}
