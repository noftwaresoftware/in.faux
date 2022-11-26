using Noftware.In.Faux.Core.Models;
using Noftware.In.Faux.Core.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Noftware.In.Faux.Data.Services
{
    /// <summary>
    /// Parses a text file that contains quote data.
    /// Format: Quote text {Meaning/explanation text} [FileName] |Comma-separated keywords/tags|
    /// Example: Circle back {Following up on progress.} [CircleBack.png] |reach,contact,follow,up,follow-up,communicate|
    /// </summary>
    public class QuoteParser : IQuoteParser
    {
        // Settings for the parser
        private readonly QuoteParserSettings _settings;

        // Quote service to use BuildSearchWords()
        private readonly IQuoteService _quoteService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings">Settings for the parser.</param>
        /// <param name="quoteService"></param>
        public QuoteParser(QuoteParserSettings settings, IQuoteService quoteService)
        {
            _settings = settings;
            _quoteService = quoteService;
        }

        /// <summary>
        /// Parses the quote text file.
        /// </summary>
        /// <returns><see cref="IAsyncEnumerable{ParsedQuote}"/></returns>
        public async IAsyncEnumerable<ParsedQuote> ParseInputFileAsync()
        {
            if (System.IO.File.Exists(_settings.QuoteTextFile) == false)
            {
                yield return null;
            }

            string cleanLine;
            int index1;
            int index2;
            string filename;
            string text;
            string description;
            string keywords;

            string[] lines = await System.IO.File.ReadAllLinesAsync(_settings.QuoteTextFile);
            foreach (var line in lines)
            {
                cleanLine = line?.Trim();
                if (string.IsNullOrEmpty(cleanLine) == false)
                {
                    // Any parsed errors
                    var parserErrors = new List<string>();

                    // One parsed quote per line in the text file
                    var quote = new ParsedQuote()
                    {
                        Line = cleanLine
                    };

                    // ex: Circle back {Following up on progress.} [CircleBack.png] |reach,contact,follow,up,follow-up,communicate|
                    if (cleanLine.Contains('{') == true && cleanLine.Contains('}') == true &&
                    cleanLine.Contains('[') == true && cleanLine.Contains(']') == true)
                    {
                        // Meaning: Ensure index2 is after index1
                        index1 = cleanLine.IndexOf("{");
                        index2 = cleanLine.IndexOf("}");
                        if (index2 > index1)
                        {
                            description = cleanLine.Substring(index1 + 1, index2 - index1 - 1)?.Trim();
                            quote.Description = description;
                        }
                        else
                        {
                            parserErrors.Add("The description's closing '}' appears before the text's opening '{'.");
                        }

                        // Quote: Get from the start to the first opening '{'
                        text = cleanLine[..(index1 - 1)];
                        quote.Text = text;

                        // Image: Ensure index2 is after index1
                        index1 = cleanLine.IndexOf("[");
                        index2 = cleanLine.IndexOf("]");
                        if (index2 > index1)
                        {
                            filename = cleanLine.Substring(index1 + 1, index2 - index1 - 1)?.Trim();
                            quote.FileName = filename;

                            LoadImage(quote);
                            if (quote.OriginalImage != null)
                            {
                                // Resized/display image
                                ResizeImage(quote, (int)_settings.MaximumResizedImageDimension, out byte[] newImage, out int newHeight, out int newWidth);
                                quote.ResizedImage = newImage;
                                quote.ResizedImageHeight = newHeight;
                                quote.ResizedImageWidth = newWidth;

                                // Thumbnail image
                                ResizeImage(quote, (int)_settings.MaximumThumbnailImageDimension, out newImage, out newHeight, out newWidth);
                                quote.ThumbnailImage = newImage;
                                quote.ThumbnailImageHeight = newHeight;
                                quote.ThumbnailImageWidth = newWidth;
                            }
                        }
                        else
                        {
                            parserErrors.Add("The file name's closing ']' appears before the file name's opening '['.");
                        }

                        // Keywords: Ensure index2 is after index1
                        index1 = cleanLine.IndexOf("|");
                        index2 = cleanLine.LastIndexOf("|");
                        if (index2 > index1)
                        {
                            keywords = cleanLine.Substring(index1 + 1, index2 - index1 - 1)?.Trim();
                            if (string.IsNullOrWhiteSpace(keywords) == false)
                            {
                                // Remove any spaces in the keywords and split on a comma
                                keywords = keywords.Replace(" ", string.Empty);
                                quote.KeyWords = keywords.Split(new char[] { ',' });
                            }
                            else
                            {
                                parserErrors.Add("The keywords are not defined between the '|' delimiters.");
                            }
                        }
                        else
                        {
                            parserErrors.Add("The keywords are not defined.");
                        }

                        // Build the search words
                        var searchWords = new List<string>();
                        if (string.IsNullOrWhiteSpace(quote.Text) == false)
                        {
                            // Get the search words from the Text
                            var items = _quoteService.BuildSearchWords(quote.Text);
                            if (items?.Any() == true)
                            {
                                searchWords.AddRange(items);
                            }
                        }
                        if (string.IsNullOrWhiteSpace(quote.Description) == false)
                        {
                            // Get the search words from the Description
                            var items = _quoteService.BuildSearchWords(quote.Description);
                            if (items?.Any() == true)
                            {
                                searchWords.AddRange(items);
                            }
                        }
                        if (quote.KeyWords?.Any() == true)
                        {
                            searchWords.AddRange(quote.KeyWords);
                        }
                        if (searchWords.Count > 0)
                        {
                            // Ensure that there are no duplicates
                            searchWords = searchWords.Distinct().ToList();
                        }
                        quote.SearchItems = searchWords;

                        yield return quote;
                    }
                } // if (string.IsNullOrEmpty(cleanLin...
            } // foreach (var line in lines)
        }

        /// <summary>
        /// Based on a filename in the quote, load the image from disk into the OriginalImage byte array.
        /// </summary>
        /// <param name="quote">Parsed quote with filename.</param>
        private void LoadImage(ParsedQuote quote)
        {
            string filename = quote.FileName;
            string imageFile = System.IO.Path.Combine(_settings.InputImagePath, filename);

            if (System.IO.File.Exists(imageFile) == false)
            {
                // File not found
                var errors = new List<string>();
                if (quote.Errors?.Any() == true)
                {
                    errors.AddRange(quote.Errors);
                }
                errors.Add($"Image '{imageFile}' not found.");
                quote.Errors = errors;

                quote.OriginalImage = null;
                quote.OriginalImageHeight = 0;
                quote.OriginalImageWidth = 0;
            }
            else
            {
                // Load file and store as byte array
                using var image = SixLabors.ImageSharp.Image.Load(imageFile, out IImageFormat format);
                quote.OriginalImageHeight = image.Height;
                quote.OriginalImageWidth = image.Width;
                using var ms = new MemoryStream();
                image.Save(ms, format);
                quote.OriginalImage = ms.ToArray();
            }
        }

        /// <summary>
        /// Resize the OriginalImage byte array to a maximum width or height and store in the ResizedImage byte array.
        /// </summary>
        /// <param name="quote">Parsed quote with populated OriginalImage.</param>
        /// <param name="maximumDimension">The maximum dimension of the resized height or width.</param>
        /// <param name="newImageBytes">The resized image.</param>
        /// <param name="newImageHeight">The height of the resized image.</param>
        /// <param name="newImageWidth">The width of the resized image.</param>
        private static void ResizeImage(ParsedQuote quote, int maximumDimension, out byte[] newImageBytes, out int newImageHeight, out int newImageWidth)
        {
            int origWidth = quote.OriginalImageWidth;
            int origHeight = quote.OriginalImageHeight;

            // Ratio between height and width
            double resizeRatio;

            // Is this a square or rectangular image?
            bool squareImage = (origWidth == origHeight);

            // If width is greater than the maximum, resize the width.
            // If height is greater than the maximum, resize the height.
            double newWidth = origWidth;
            double newHeight = origHeight;
            if (squareImage == true)
            {
                // Square image
                if (origWidth > maximumDimension)
                {
                    newWidth = maximumDimension;
                    newHeight = maximumDimension;
                }
            }
            else
            {
                // Rectangular image
                if (origWidth > origHeight)
                {
                    // Landscape: Resize the width
                    if (origWidth > maximumDimension)
                    {
                        resizeRatio = (double)maximumDimension / (double)origWidth;
                        newWidth = maximumDimension;
                        newHeight = origHeight * resizeRatio;
                    }
                }
                else
                {
                    // Portrait: Resize the height
                    if (origHeight > maximumDimension)
                    {
                        resizeRatio = (double)maximumDimension / (double)origHeight;
                        newHeight = maximumDimension;
                        newWidth = origWidth * resizeRatio;
                    }
                }
            }

            newImageHeight = (int)newHeight;
            newImageWidth = (int)newWidth;

            using var image = SixLabors.ImageSharp.Image.Load(quote.OriginalImage, out IImageFormat format);
            int height = newImageHeight;
            int width = newImageWidth;

            image.Mutate(x => x
                 .Resize(width, height));

            using var ms = new MemoryStream();
            image.Save(ms, format);
            newImageBytes = ms.ToArray();
        }
    }
}
