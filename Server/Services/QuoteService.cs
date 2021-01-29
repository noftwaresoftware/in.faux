using Noftware.In.Faux.Shared.Services;
using Noftware.In.Faux.Server.Azure;
using Noftware.In.Faux.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noftware.In.Faux.Shared.Extensions;
using System.Security.Cryptography;
using System.IO;
using Noftware.In.Faux.Server.Azure.Entities;
using Noftware.In.Faux.Shared.Data;
using System.Text;

namespace Noftware.In.Faux.Server.Services
{
    /// <summary>
    /// Service for obtaining quote items from the data store.
    /// </summary>
    public class QuoteService : IQuoteService
    {
        // Azure Table repository for quotes
        private readonly ITableRepository<QuoteTableEntity> _quoteTableRepository;

        // Azure Table repository for the quote search index
        private readonly ITableRepository<QuoteSearchIndexTableEntity> _quoteSearchTableRepository;

        // Azure file share repository for resized quote images
        private readonly IFileShareRepository<ResizedImageFile> _resizedImageFileRepository;

        // Azure file share repository for thumbnail quote images
        private readonly IFileShareRepository<ThumbnailImageFile> _thumbnailImageFileRepository;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="quoteTableRepository">Azure Table repository for quotes.</param>
        /// <param name="quoteSearchTableRepository">Azure Table repository for the quote search index.</param>
        /// <param name="resizedImageFileRepository">Azure file share repository for resized quote images.</param>
        /// <param name="thumbnailImageFileRepository">Azure file share repository for thumbnail quote images.</param>
        public QuoteService(ITableRepository<QuoteTableEntity> quoteTableRepository,
            ITableRepository<QuoteSearchIndexTableEntity> quoteSearchTableRepository,
            IFileShareRepository<ResizedImageFile> resizedImageFileRepository,
            IFileShareRepository<ThumbnailImageFile> thumbnailImageFileRepository)
        {
            _quoteTableRepository = quoteTableRepository;
            _quoteSearchTableRepository = quoteSearchTableRepository;

            _resizedImageFileRepository = resizedImageFileRepository;
            _thumbnailImageFileRepository = thumbnailImageFileRepository;
        }

        /// <summary>
        /// Get a random quote from the data store.
        /// </summary>
        /// <returns><see cref="Task{Quote}"/></returns>
        public async Task<Quote> GetRandomQuoteAsync()
        {
            string[] selectFields = GetQuotePublicSelectFields();

            var quoteEntity = await _quoteTableRepository.GetRandomAsync(selectFields);
            var resizedFile = await _resizedImageFileRepository.GetFileAsync(quoteEntity.FileName);
            var thumbnailFile = await _thumbnailImageFileRepository.GetFileAsync(quoteEntity.FileName);

            var quote = new Quote()
            {
                Description = quoteEntity.Description,
                Key = quoteEntity.RowKey.ConvertTo<Guid>(),
                Text = quoteEntity.Text,
                FileName = quoteEntity.FileName,
                Base64Image = (resizedFile is null ? null : System.Convert.ToBase64String(resizedFile.Contents)),
                Base64ThumbnailImage = (thumbnailFile is null ? null : System.Convert.ToBase64String(thumbnailFile.Contents))
            };

            return quote;
        }

        /// <summary>
        /// Get a thumbnail image from the file share
        /// </summary>
        /// <param name="fileName">Name of file.</param>
        /// <returns><see cref="Task{string}"/></returns>
        public async Task<string> GetResizedImageAsync(string fileName)
        {
            var file = await _resizedImageFileRepository.GetFileAsync(fileName);
            string base64Image = (file is null ? null : System.Convert.ToBase64String(file.Contents));

            return base64Image;
        }

        /// <summary>
        /// Search for quotes via one or more words. Words should be separated by spaces.
        /// </summary>
        /// <param name="searchPhrase">Search word(s) separated by spaces.</param>
        /// <returns><see cref="Task{IEnumerable{Quote}}"/></returns>
        public async Task<IEnumerable<Quote>> SearchQuotesAsync(string searchPhrase)
        {
            if (string.IsNullOrWhiteSpace(searchPhrase) == true)
            {
                return default;
            }

            // Build the search words
            var searchWords = this.BuildSearchWords(searchPhrase);

            // Search fields
            string[] selectFieldsSearch = this.GetQuoteSearchPublicSelectFields();

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
            string[] selectFieldsQuote = this.GetQuotePublicSelectFields();

            // Searched quotes
            var quotes = new List<Quote>();

            // Get all matches, add the ones with the most matches at the top, and get the thumbnail image
            foreach (var groupedSearchWord in groupedSearchWords)
            {
                // Get the quote
                var quoteEntity = await _quoteTableRepository.Get(groupedSearchWord.QuoteRowKey, selectFieldsQuote);
                if (quoteEntity is not null)
                {
                    // Get the thumbnail and build the return quote
                    var resizedFile = await _resizedImageFileRepository.GetFileAsync(quoteEntity.FileName);
                    var thumbnailFile = await _thumbnailImageFileRepository.GetFileAsync(quoteEntity.FileName);

                    var quote = new Quote()
                    {
                        Description = quoteEntity.Description,
                        Key = quoteEntity.RowKey.ConvertTo<Guid>(),
                        Text = quoteEntity.Text,
                        FileName = quoteEntity.FileName,
                        //Base64Image = (resizedFile is null ? null : System.Convert.ToBase64String(resizedFile.Contents)),
                        Base64ThumbnailImage = (thumbnailFile is null ? null : System.Convert.ToBase64String(thumbnailFile.Contents))
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
        /// <returns><see cref="IEnumerable{string}"/> or null, if input is empty.</returns>
        private IEnumerable<string> BuildSearchWords(string input)
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
        /// Get only the list of quote fields required to populate on the public UI.
        /// </summary>
        /// <returns><see cref="string[]"/></returns>
        private string[] GetQuotePublicSelectFields()
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
        /// Get only the list of quote search fields required to populate on the public UI.
        /// </summary>
        /// <returns><see cref="string[]"/></returns>
        private string[] GetQuoteSearchPublicSelectFields()
        {
            string[] selectFields = new string[] {
                nameof(QuoteSearchIndexTableEntity.QuoteRowKey),
                nameof(QuoteSearchIndexTableEntity.Word)
            };

            return selectFields;
        }
    }
}
