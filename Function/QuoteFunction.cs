// Ignore Spelling: Noftware Faux req

using System.Dynamic;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Noftware.In.Faux.Core.Models;
using Noftware.In.Faux.Core.Services;
using Noftware.In.Faux.Data.Azure.Entities;

namespace Noftware.In.Faux.Function
{
    /// <summary>
    /// Get a random quote.
    /// </summary>
    /// <param name="quoteService">Quote service.</param>
    public class QuoteFunction(IQuoteService quoteService)
    {
        /// <summary>
        /// Get a random quote.
        /// </summary>
        /// <param name="req">Represents the incoming side of an individual HTTP request.</param>
        /// <param name="executionContext">Encapsulates the information about a function execution.</param>
        /// <returns><see cref="Quote"/></returns>
        [Function("randomquote")]
        public async Task<HttpResponseData> GetRandomQuote([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(GetRandomQuote));
            string caller = $"{typeof(QuoteFunction).Name}.{nameof(GetRandomQuote)}";
            logger.LogInformation("{Message}", caller);

            var quote = await quoteService.GetRandomQuoteAsync();

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync<Quote>(quote);
            return response;
        }

        /// <summary>
        /// Get a Base 64 encoded resized image.
        /// </summary>
        /// <param name="req">HTTP request.</param>
        /// <param name="executionContext">Encapsulates the information about a function execution.</param>
        /// <returns><see cref="string"/></returns>
        /// <remarks>
        /// Posted body format:
        /// {
        ///   "quoteRowKey": "8675309",
        ///   "fileName": "JennyJenny.png"
        /// }
        /// </remarks>
        [Function("resizedimage")]
        public async Task<HttpResponseData> GetResizedImage(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(GetResizedImage));
            string caller = $"{typeof(QuoteFunction).Name}.{nameof(GetResizedImage)}";
            logger.LogInformation("{Message}", caller);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<ExpandoObject>(requestBody);
            var quoteKey = data?.FirstOrDefault(f => f.Key.Equals(nameof(QuoteImpressionTableEntity.QuoteRowKey), StringComparison.CurrentCultureIgnoreCase)).Value?.ToString();
            var fileName = data?.FirstOrDefault(f => f.Key.Equals(nameof(ParsedQuote.FileName), StringComparison.CurrentCultureIgnoreCase)).Value?.ToString();

            var base64Image = await quoteService.GetResizedImageAsync(quoteKey, fileName);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync(base64Image);
            return response;
        }

        /// <summary>
        /// Search the quotes.
        /// </summary>
        /// <param name="req">HTTP request.</param>
        /// <param name="executionContext">Encapsulates the information about a function execution.</param>
        /// <returns><see cref="IEnumerable{Quote}"/></returns>
        /// <remarks>
        /// Posted body format:
        /// {
        ///   "phrase": "tommy tutone"
        /// }
        /// </remarks>
        [Function("search")]
        public async Task<HttpResponseData> SearchQuotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger(nameof(SearchQuotes));
            string caller = $"{typeof(QuoteFunction).Name}.{nameof(SearchQuotes)}";
            logger.LogInformation("{Message}", caller);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<ExpandoObject>(requestBody);
            string? searchPhrase = data?.FirstOrDefault(f => f.Key.Equals("phrase", StringComparison.CurrentCultureIgnoreCase)).Value?.ToString();

            var searchResults = await quoteService.SearchQuotesAsync(searchPhrase);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync<IEnumerable<Quote>>(searchResults);
            return response;
        }
    }
}
