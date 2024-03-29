﻿// Ignore Spelling: Noftware Faux

using System.Collections.Generic;
using System.Threading.Tasks;
using Noftware.In.Faux.Client.ViewModels;

namespace Noftware.In.Faux.Client.Services
{
    /// <summary>
    /// Client quote service interface.
    /// </summary>
    public interface IClientQuoteService
    {
        /// <summary>
        /// Get a random quote from the data store and map to the view model.
        /// </summary>
        /// <returns><see cref="ViewQuote"/></returns>
        Task<ViewQuote> GetRandomQuoteAsync();

        /// <summary>
        /// Get a resized (display) image from the file share
        /// </summary>
        /// <param name="quoteKey">Quote key unique identifier.</param>
        /// <param name="fileName">Name of file.</param>
        /// <returns><see cref="string"/></returns>
        Task<string> GetResizedImageAsync(string quoteKey, string fileName);

        /// <summary>
        /// Search for quotes based on a space-delimited phrase and map to view models.
        /// </summary>
        /// <param name="searchPhrase">Words delimited by spaces.</param>
        /// <returns><see cref="IEnumerable{ViewQuote}"/></returns>
        Task<IEnumerable<ViewQuote>> SearchQuotesAsync(string searchPhrase);
    }
}
