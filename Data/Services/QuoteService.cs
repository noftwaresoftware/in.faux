using Noftware.In.Faux.Core.Services;
using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noftware.In.Faux.Core.Extensions;
using System.Security.Cryptography;
using System.IO;
using Noftware.In.Faux.Data.Azure.Entities;
using Noftware.In.Faux.Core.Data;
using System.Text;

namespace Noftware.In.Faux.Data.Services
{
    /// <summary>
    /// Service for obtaining quote items from the data store.
    /// </summary>
    public class QuoteService : IQuoteService
    {
        // Azure Table repository for quotes
        private readonly ITableRepository<QuoteTableEntity> _quoteTableRepository;

        // Azure Table repository for a single quote metadata item
        private readonly ITableRepository<QuoteMetadataTableEntity> _quoteTableMetadataRepository;

        // Azure Table repository for the quote search index
        private readonly ITableRepository<QuoteSearchIndexTableEntity> _quoteSearchTableRepository;

        // Azure Table repository for the quote search index
        private readonly ITableRepository<QuoteImpressionTableEntity> _quoteImpressionTableRepository;

        // Azure file share repository for resized quote images
        private readonly IFileShareRepository<ResizedImageFile> _resizedImageFileRepository;

        // Azure file share repository for thumbnail quote images
        private readonly IFileShareRepository<ThumbnailImageFile> _thumbnailImageFileRepository;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="quoteTableRepository">Azure Table repository for quotes.</param>
        /// <param name="quoteTablemetaDataRepository">Azure Table repository for a single quote metadata item.</param>
        /// <param name="quoteSearchTableRepository">Azure Table repository for the quote search index.</param>
        /// <param name="quoteImpressionTableRepository">Azure Table repository for the quote impressions.</param>
        /// <param name="resizedImageFileRepository">Azure file share repository for resized quote images.</param>
        /// <param name="thumbnailImageFileRepository">Azure file share repository for thumbnail quote images.</param>
        public QuoteService(ITableRepository<QuoteTableEntity> quoteTableRepository,
            ITableRepository<QuoteMetadataTableEntity> quoteTablemetaDataRepository,
            ITableRepository<QuoteSearchIndexTableEntity> quoteSearchTableRepository,
            ITableRepository<QuoteImpressionTableEntity> quoteImpressionTableRepository,
            IFileShareRepository<ResizedImageFile> resizedImageFileRepository,
            IFileShareRepository<ThumbnailImageFile> thumbnailImageFileRepository)
        {
            _quoteTableRepository = quoteTableRepository;
            _quoteTableMetadataRepository = quoteTablemetaDataRepository;
            _quoteSearchTableRepository = quoteSearchTableRepository;
            _quoteImpressionTableRepository = quoteImpressionTableRepository;

            _resizedImageFileRepository = resizedImageFileRepository;
            _thumbnailImageFileRepository = thumbnailImageFileRepository;
        }

        /// <summary>
        /// Get a random quote from the data store.
        /// </summary>
        /// <returns><see cref="Quote"/></returns>
        public async Task<Quote> GetRandomQuoteAsync()
        {
            // This is only a single quote metadata record
            var quoteMetadata = await _quoteTableMetadataRepository.GetAsync("1", GetQuoteMetadataSelectFields());
            if (quoteMetadata is null)
            {
                return null;
            }

            // Total quote count
            int totalQuoteCount = quoteMetadata.QuoteTotalCount;

            // Get a random number between 1 and 'total quote count'
            int randomQuoteKey = ThreadSafeRandom.Next(1, totalQuoteCount + 1);

            // Get the quote based on row key
            var quoteEntity = await _quoteTableRepository.GetAsync(randomQuoteKey.ToString(), GetQuoteSelectFields());
            if (quoteEntity is null)
            {
                return null;
            }

            string resizedFileBase64 = await this.GetResizedImageAsync(quoteEntity.RowKey, quoteEntity.FileName);

            var quote = new Quote()
            {
                Description = quoteEntity.Description,
                Key = quoteEntity.RowKey,
                Text = quoteEntity.Text,
                FileName = quoteEntity.FileName,
                Base64Image = resizedFileBase64
            };

            return quote;
        }

        /// <summary>
        /// Get a thumbnail image from the file share
        /// </summary>
        /// <param name="quoteKey">Quote key unique identifier.</param>
        /// <param name="fileName">Name of file.</param>
        /// <returns><see cref="string"/></returns>
        public async Task<string> GetResizedImageAsync(string quoteKey, string fileName)
        {
            var file = await _resizedImageFileRepository.GetFileAsync(fileName);
            string base64Image = (file is null ? null : System.Convert.ToBase64String(file.Contents));

            // Add a new impression/view
            await _quoteImpressionTableRepository.AddOrUpdateAsync(new QuoteImpressionTableEntity()
            {
                QuoteRowKey = quoteKey,
                PartitionKey = "QuoteImpression",
                RowKey = System.Guid.NewGuid().ToString(),
                Timestamp = DateTimeOffset.UtcNow
            });

            return base64Image;
        }

        /// <summary>
        /// Search for quotes via one or more words. Words should be separated by spaces.
        /// </summary>
        /// <param name="searchPhrase">Search word(s) separated by spaces.</param>
        /// <returns><see cref="IEnumerable{Quote}"/></returns>
        public async Task<IEnumerable<Quote>> SearchQuotesAsync(string searchPhrase)
        {
            if (string.IsNullOrWhiteSpace(searchPhrase) == true)
            {
                return default;
            }

            // Build the search words
            var searchWords = this.BuildSearchWords(searchPhrase);

            // Search fields
            string[] selectFieldsSearch = GetQuoteSearchSelectFields();

            // Build a list of word filters (Note: PartitionKey is already present)
            var filters = new List<TableEntityFilter>();
            bool firstFilterSet = false;
            foreach (var searchWord in searchWords)
            {
                // The first filter is an AND and the others are OR's, This is
                // because the PartitionKey is already included first so one
                // of these search words must be matched with the PartitionKey.
                if (firstFilterSet == false)
                {
                    // First: AND
                    filters.Add(new TableEntityFilter()
                    {
                        BooleanOperator = BooleanOperator.And,
                        Field = nameof(QuoteSearchIndexTableEntity.Word),
                        ComparisonOperator = ComparisonOperator.EqualTo,
                        Value = $"'{searchWord}'"
                    });
                    firstFilterSet = true;
                }
                else
                {
                    // All others: OR
                    filters.Add(new TableEntityFilter()
                    {
                        BooleanOperator = BooleanOperator.Or,
                        Field = nameof(QuoteSearchIndexTableEntity.Word),
                        ComparisonOperator = ComparisonOperator.EqualTo,
                        Value = $"'{searchWord}'"
                    });
                    firstFilterSet = true;
                }
            } // foreach (var searchWord in s...

            // If there are no filters, do not return any results.
            if (filters.Count == 0)
            {
                return default;
            }

            // Search the search index for all matches and get a count of items by matches 
            var searchedWords = await _quoteSearchTableRepository.SearchAsync(filters, selectFieldsSearch).ToListAsync();
            var groupedSearchWords = searchedWords.GroupBy(g => g.QuoteRowKey)
                                        .Select(group => new
                                        {
                                            QuoteRowKey = group.Key,
                                            Count = group.Count()
                                        })
                                        .OrderByDescending(x => x.Count);

            // Quote fields
            string[] selectFieldsQuote = GetQuoteSelectFields();

            // Searched quotes
            var quotes = new List<Quote>();

            // Get all matches, add the ones with the most matches at the top, and get the thumbnail image
            foreach (var groupedSearchWord in groupedSearchWords)
            {
                // Get the quote
                var quoteEntity = await _quoteTableRepository.GetAsync(groupedSearchWord.QuoteRowKey, selectFieldsQuote);
                if (quoteEntity != null)
                {
                    // Get the thumbnail and build the return quote
                    var resizedFile = await _resizedImageFileRepository.GetFileAsync(quoteEntity.FileName);
                    var thumbnailFile = await _thumbnailImageFileRepository.GetFileAsync(quoteEntity.FileName);

                    var quote = new Quote()
                    {
                        Description = quoteEntity.Description,
                        Key = quoteEntity.RowKey,
                        Text = quoteEntity.Text,
                        FileName = quoteEntity.FileName
                    };
                    quotes.Add(quote);
                }
            }

            return quotes;
        }

        /// <summary>
        /// Build the search word index by filtering out characters/words that should not be indexed, such as punctuation and whitespace.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns><see cref="IEnumerable{String}"/> or null, if input is empty.</returns>
        public IEnumerable<string> BuildSearchWords(string input)
        {
            // Empty string
            if (string.IsNullOrWhiteSpace(input) == true)
            {
                return default;
            }

            // Return items
            var items = new List<string>();

            // Make all search words lowercase
            input = input.Trim().ToLowerInvariant();

            // Each word
            var word = new StringBuilder();

            // Iterate over each character to determine if it is part of the word or the end of the word
            foreach (var c in input)
            {
                if (char.IsLetterOrDigit(c) == true)
                {
                    word.Append(c);
                }
                else if (char.IsControl(c) == true || char.IsPunctuation(c) == true || char.IsSeparator(c) == true || char.IsWhiteSpace(c) == true)
                {
                    // End of word
                    if (word.Length > 0)
                    {
                        items.Add(word.ToString());
                        word = new StringBuilder();
                    }
                }
            } // foreach (var c in input)

            // Ensure that the last word is captured
            if (word.Length > 0)
            {
                items.Add(word.ToString());
            }

            // Remove any duplicates
            if (items.Count > 0)
            {
                items = items.Distinct().ToList();
            }

            return items;
        }

        /// <summary>
        /// Get only the list of quote fields required for this service.
        /// </summary>
        /// <returns>Select columns.</returns>
        private static string[] GetQuoteSelectFields()
        {
            string[] selectFields = new string[] {
                nameof(QuoteTableEntity.Description),
                nameof(QuoteTableEntity.RowKey),
                nameof(QuoteTableEntity.FileName),
                nameof(QuoteTableEntity.Text)
            };

            return selectFields;
        }

        /// <summary>
        /// Get only the list of quote metadata fields required for this service.
        /// </summary>
        /// <returns>Select fields.</returns>
        private static string[] GetQuoteMetadataSelectFields()
        {
            string[] selectFields = new string[] {
                nameof(QuoteMetadataTableEntity.QuoteTotalCount)
            };

            return selectFields;
        }

        /// <summary>
        /// Get only the list of quote search fields required for this service.
        /// </summary>
        /// <returns>Select fields.</returns>
        private static string[] GetQuoteSearchSelectFields()
        {
            string[] selectFields = new string[] {
                nameof(QuoteSearchIndexTableEntity.QuoteRowKey),
                nameof(QuoteSearchIndexTableEntity.Word)
            };

            return selectFields;
        }
    }
}
