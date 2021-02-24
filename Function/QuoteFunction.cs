using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Noftware.In.Faux.Core.Services;
using System.Collections.Generic;
using Noftware.In.Faux.Core.Models;

namespace Noftware.In.Faux.Function
{
    /// <summary>
    /// Get a random quote.
    /// </summary>
    public class QuoteFunction
    {
        // Quote service
        private readonly IQuoteService _quoteService;

        /// <summary>
        /// DI constructor.
        /// </summary>
        /// <param name="quoteService">Quote service.</param>
        public QuoteFunction(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        /// <summary>
        /// Get a random quote.
        /// </summary>
        /// <param name="req">Represents the incoming side of an individual HTTP request.</param>
        /// <param name="log">Logger.</param>
        /// <returns><see cref="Quote"/></returns>
        [FunctionName("randomquote")]
        public async Task<IActionResult> GetRandomQuote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            log?.LogInformation("randomquote function request.");

            var quote = await _quoteService.GetRandomQuoteAsync();

            return new OkObjectResult(quote);
        }

        /// <summary>
        /// Get a Base 64 encoded resized image.
        /// </summary>
        /// <param name="req">HTTP request.</param>
        /// <param name="log">Logger.</param>
        /// <returns><see cref="string"/></returns>
        /// <remarks>
        /// Posted body format:
        /// {
        ///   "quoteRowKey": "8675309",
        ///   "fileName": "JennyJenny.png"
        /// }
        /// </remarks>
        [FunctionName("resizedimage")]
        public async Task<IActionResult> GetResizedImage(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                ILogger log)
        {
            log?.LogInformation("resizedimage function request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string quoteKey = data?.quoteRowKey;
            string fileName = data?.fileName;

            var base64Image = await _quoteService.GetResizedImageAsync(quoteKey, fileName);

            return new OkObjectResult(base64Image);
        }

        /// <summary>
        /// Search the quotes.
        /// </summary>
        /// <param name="req">HTTP request.</param>
        /// <param name="log">Logger.</param>
        /// <returns><see cref="IEnumerable{Quote}"/></returns>
        /// <remarks>
        /// Posted body format:
        /// {
        ///   "phrase": "tommy tutone"
        /// }
        /// </remarks>
        [FunctionName("search")]
        public async Task<IActionResult> SearchQuotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log?.LogInformation("search function request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string searchPhrase = data?.phrase;

            var searchResults = await _quoteService.SearchQuotesAsync(searchPhrase);

            return new OkObjectResult(searchResults);
        }
    }
}
