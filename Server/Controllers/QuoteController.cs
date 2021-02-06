using Noftware.In.Faux.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noftware.In.Faux.Shared.Services;
using Noftware.In.Faux.Shared.Extensions;
using Noftware.In.Faux.Client.ViewModels;

namespace Noftware.In.Faux.Server.Controllers
{
    /// <summary>
    /// Quote API controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class QuoteController : ControllerBase
    {
        // Quote service
        private readonly IQuoteService _quoteService;

        // Logger
        private readonly ILogger<QuoteController> _logger;

        /// <summary>
        /// DI constructor.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> interface.</param>
        /// <param name="quoteService"><see cref="IQuoteService"/> interface.</param>
        public QuoteController(ILogger<QuoteController> logger, IQuoteService quoteService)
        {
            _logger = logger;
            _quoteService = quoteService;
        }

        /// <summary>
        /// Get a random quote from the data store.
        /// </summary>
        /// <returns><see cref="Quote"/></returns>
        [HttpGet]
        public async Task<Quote> GetAsync()
        {
            Quote quote = null;
            try
            {
                quote = await _quoteService.GetRandomQuoteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred retrieving a random quote.");
            }

            return quote;
        }

        /// <summary>
        /// Search for quotes based on space-delimited words.
        /// </summary>
        /// <param name="searchPhrase">Words delimited by spaces.</param>
        /// <returns><see cref="IEnumerable{Quote}"/></returns>
        [HttpPost]
        [Route("search")]
        public async Task<IEnumerable<Quote>> SearchAsync(ViewQuoteSearch searchPhrase)
        {
            var quotes = await _quoteService.SearchQuotesAsync(searchPhrase.Phrase);
            return quotes;
        }

        /// <summary>
        /// Get a resized image from the file share.
        /// </summary>
        /// <param name="fileName">Name of file.</param>
        /// <returns><see cref="Task{string}"/></returns>
        [HttpPost]
        [Route("resizedimage")]
        public async Task<string> GetResizedImageAsync(ViewQuoteFileName fileName)
        {
            var base64Image = await _quoteService.GetResizedImageAsync(fileName.QuoteRowKey, fileName.FileName);
            return base64Image;
        }
    }
}
