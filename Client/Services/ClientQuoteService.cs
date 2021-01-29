using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Noftware.In.Faux.Client.ViewModels;
using Noftware.In.Faux.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Client.Services
{
    /// <summary>
    /// Client quote service interface.
    /// </summary>
    public class ClientQuoteService : IClientQuoteService
    {
        // HTTP client to call the API.
        private readonly HttpClient _httpClient;

        // Represents a set of key/value application configuration properties
        private readonly IConfiguration _configuration;

        // Logger
        private readonly ILogger _logger;

        // HTTP retries
        private readonly int _httpRetries = 5;

        /// <summary>
        /// DI constructor.
        /// </summary>
        /// <param name="httpClient">HTTP client to call the API.</param>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        /// <param name="logger">Logger.</param>
        public ClientQuoteService(HttpClient httpClient, IConfiguration configuration, ILogger<IClientQuoteService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            // Set the HTTP Retries
            int.TryParse(_configuration["RequestRetries"], out _httpRetries);
        }

        /// <summary>
        /// Get a random quote from the data store and map to the view model.
        /// </summary>
        /// <returns><see cref="Task{ViewQuote}"/></returns>
        public async Task<ViewQuote> GetRandomQuoteAsync()
        {
            Quote quote = null;

            int count = 0;
            do
            {
                try
                {
                    quote = await _httpClient.GetFromJsonAsync<Quote>("Quote");
                    count = _httpRetries + 1;
                }
                catch (JsonException ex)
                {
                    _logger?.LogError(ex, "A JSON error occurred in fetching a random quote.");
                }

                count++;

            } while (count < _httpRetries);


            // If quote is still empty, get the default out-of-the-box one
            if (quote is null)
            {
                quote = Quote.GetDefault();
            }

            // Map to view model
            if (quote is not null)
            {
                var viewQuote = new ViewQuote()
                {
                    Base64Image = quote.Base64Image,
                    Base64ThumbnailImage = quote.Base64ThumbnailImage,
                    Description = quote.Description,
                    FileName = quote.FileName,
                    Key = quote.Key,
                    Text = quote.Text
                };

                return viewQuote;
            }

            return null;
        }

        /// <summary>
        /// Get a resized (display) image from the file share
        /// </summary>
        /// <param name="fileName">Name of file.</param>
        /// <returns><see cref="Task{string}"/></returns>
        public async Task<string> GetResizedImageAsync(string fileName)
        {
            string base64Image = null;

            int count = 0;
            do
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("Quote/resizedimage", new ViewQuoteFileName() { FileName = fileName });
                    base64Image = await response.Content.ReadAsStringAsync();

                    count = _httpRetries + 1;
                }
                catch (JsonException ex)
                {
                    _logger?.LogError(ex, "A JSON error occurred in fetching a resized image.");
                }

                count++;

            } while (count < _httpRetries);

            return base64Image;
        }

        /// <summary>
        /// Search for quotes based on a space-delimited phrase and map to view models.
        /// </summary>
        /// <param name="searchPhrase">Words delimited by spaces.</param>
        /// <returns><see cref="Task{IEnumerable{ViewQuote}}"/></returns>
        public async Task<IEnumerable<ViewQuote>> SearchQuotesAsync(string searchPhrase)
        {
            var quotes = new List<ViewQuote>();

            int count = 0;
            do
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("Quote/search", new ViewQuoteSearch() { Phrase = searchPhrase });

                    var searchedQuotes = await response.Content.ReadFromJsonAsync<IEnumerable<Quote>>();
                    if (searchedQuotes?.Any() == true)
                    {
                        foreach (var searchedQuote in searchedQuotes)
                        {
                            quotes.Add(new ViewQuote()
                            {
                                Base64Image = searchedQuote.Base64Image,
                                Base64ThumbnailImage = searchedQuote.Base64ThumbnailImage,
                                Description = searchedQuote.Description,
                                FileName = searchedQuote.FileName,
                                Key = searchedQuote.Key,
                                Text = searchedQuote.Text
                            });
                        }
                    }

                    count = _httpRetries + 1;
                }
                catch (JsonException ex)
                {
                    _logger?.LogError(ex, "A JSON error occurred in searching quotes.");
                }

                count++;

            } while (count < _httpRetries);

            return quotes;
        }
    }
}
