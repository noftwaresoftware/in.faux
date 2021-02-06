using Noftware.In.Faux.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Shared.Services
{
    /// <summary>
    /// Quote service interface.
    /// </summary>
    public interface IQuoteService
    {
        /// <summary>
        /// Get a random quote item.
        /// </summary>
        /// <returns><see cref="Task{Quote}"/></returns>
        Task<Quote> GetRandomQuoteAsync();

        /// <summary>
        /// Get a resized (display) image from the file share
        /// </summary>
        /// <param name="quoteKey">Quote key unique identifier.</param>
        /// <param name="fileName">Name of file.</param>
        /// <returns><see cref="Task{string}"/></returns>
        Task<string> GetResizedImageAsync(string quoteKey, string fileName);

        /// <summary>
        /// Search for quotes based on a space-delimited phrase.
        /// </summary>
        /// <param name="searchPhrase">Words delimited by spaces.</param>
        /// <returns><see cref="IEnumerable{Quote}"/></returns>
        Task<IEnumerable<Quote>> SearchQuotesAsync(string searchPhrase);

        /// <summary>
        /// Build the search word index by filtering out characters/words that should not be indexed, such as punctuation and whitespace.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns><see cref="IEnumerable{string}"/> or null, if input is empty.</returns>
        IEnumerable<string> BuildSearchWords(string input);
    }
}
