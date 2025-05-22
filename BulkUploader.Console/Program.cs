// Ignore Spelling: Noftware Faux Uploader

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noftware.In.Faux.Core.Data;
using Noftware.In.Faux.Core.Extensions;
using Noftware.In.Faux.Core.Models;
using Noftware.In.Faux.Core.Services;
using Noftware.In.Faux.Data.Azure.Entities;
using Noftware.In.Faux.Data.Repositories;
using Noftware.In.Faux.Data.Services;

namespace Noftware.In.Faux.BulkUploader
{
    /// <summary>
    /// The application's main entry point.
    /// </summary>
    public class Program
    {
        // Capture original color to reset
        private static readonly ConsoleColor OriginalConsoleForegroundColor = Console.ForegroundColor;

        /// <summary>
        /// The application's main method.
        /// </summary>
        /// <param name="args">Optional command-line arguments.</param>
        public static async Task Main(string[] args)
        {
            ConsoleInformationWriteLine("[ Quote data and image Azure persistence for Noftware in.faux ]");
            ConsoleInformationWriteLine();

            var services = Configure();

            // Output the settings
            bool errorsDetected = false;
            var quoteParserSettings = services.GetService<QuoteParserSettings>();
            ConsoleInformationWriteLine("~~~~~~~~~~");
            ConsoleInformationWriteLine($"My local quote image directory: {quoteParserSettings.InputImagePath}");
            if (System.IO.Directory.Exists(quoteParserSettings.InputImagePath) == false)
            {
                errorsDetected = true;
                ConsoleErrorWriteLine("Error: Image directory not found.");
            }
            Console.WriteLine($"My local quote source text file: {quoteParserSettings.QuoteTextFile}");
            if (System.IO.File.Exists(quoteParserSettings.QuoteTextFile) == false)
            {
                errorsDetected = true;
                ConsoleErrorWriteLine("Error: Quote text file not found.");
            }
            Console.WriteLine($"Maximum resized image height or width: {quoteParserSettings.MaximumResizedImageDimension}");
            if (quoteParserSettings.MaximumResizedImageDimension <= 0)
            {
                errorsDetected = true;
                ConsoleErrorWriteLine("Error: Image dimension must be greater than zero.");
            }
            Console.WriteLine($"Maximum thumbnail image height or width: {quoteParserSettings.MaximumThumbnailImageDimension}");
            if (quoteParserSettings.MaximumThumbnailImageDimension <= 0)
            {
                errorsDetected = true;
                ConsoleErrorWriteLine("Error: Thumbnail dimension must be greater than zero.");
            }
            ConsoleInformationWriteLine("~~~~~~~~~~");
            ConsoleInformationWriteLine();

            // If settings errors were detected, do not continue
            if (errorsDetected == true)
            {
                Environment.Exit(1);
                return;
            }

            // Overwrite (or new) or Append more quotes?
            PersistenceMode mode;

            // If arguments are specified, ensure they are valid
            if (args?.Length > 0)
            {
                // Only a single argument is supported
                if (args.Length > 1)
                {
                    ConsoleErrorWriteLine("Error: Only a single argument is supported.");
                    foreach (var item in Helpers.GetArgumentInformation())
                    {
                        ConsoleErrorWriteLine(item);
                    }

                    Environment.Exit(2);
                    return;
                }
                else
                {
                    // Get the arg and validate
                    string arg = args[0].ToLowerInvariant();
                    if (ValidateArgument(arg, out string errorMessage, out mode) == false)
                    {
                        ConsoleErrorWriteLine($"Error: {errorMessage}");
                        foreach (var item in Helpers.GetArgumentInformation())
                        {
                            ConsoleErrorWriteLine(item);
                        }

                        Environment.Exit(3);
                        return;
                    }
                }
            }
            else
            {
                // Get the mode from the user and validate
                ConsoleInformationWriteLine("Which mode would you like to use? Specify a mode and then press enter on your keyboard.");
                foreach (var item in Helpers.GetInputInformation())
                {
                    ConsoleInformationWriteLine(item);
                }

                string input = Console.ReadLine();
                if (ValidateArgument(input, out string errorMessage, out mode) == false)
                {
                    ConsoleErrorWriteLine($"Error: {errorMessage}");

                    Environment.Exit(4);
                    return;
                }
            }

            // ~~~~~
            // [ All inputs were validated. Begin processing. ]
            // ~~~~~
            int quoteRowCounter;

            ConsoleInformationWriteLine();
            ConsoleInformationWriteLine($"Mode: {mode}");
            ConsoleInformationWriteLine();

            // Get the quote metadata to determine the count
            var tableRepoQuoteMetadata = services.GetService<ITableRepository<QuoteMetadataTableEntity>>();
            tableRepoQuoteMetadata.StatusUpdate += TableRepositoryOnStatusUpdate;
            var quoteMetadata = await tableRepoQuoteMetadata.GetAsync("1");
            if (quoteMetadata == null || mode == PersistenceMode.Overwrite)
            {
                // If there is no Metadata row or the user chose 'overwrite', start net-new
                // =====
                // Set the row counter. It will be updated again once all data is processed.
                quoteRowCounter = 0;

                quoteMetadata = new QuoteMetadataTableEntity()
                {
                    ETag = Azure.ETag.All,
                    PartitionKey = "QuoteMetadata",
                    QuoteTotalCount = quoteRowCounter,
                    RowKey = "1",                           // There is only ever a single row
                    Timestamp = DateTimeOffset.UtcNow
                };
                await tableRepoQuoteMetadata.AddOrUpdateAsync(quoteMetadata);
            }
            else
            {
                // Get the current total count
                quoteRowCounter = quoteMetadata.QuoteTotalCount;
            }

            // Quote table repository
            var tableRepoQuote = services.GetService<ITableRepository<QuoteTableEntity>>();
            tableRepoQuote.StatusUpdate += TableRepositoryOnStatusUpdate;
            if (mode == PersistenceMode.Overwrite)
            {
                // Delete and recreate
                ConsoleInformationWriteLine("Deleting and re-creating the quote table repository.");
                await tableRepoQuote.DeleteTableAsync();
                await tableRepoQuote.CreateTableAsync();
            }

            // Quote search index table repository
            var tableRepoQuoteSearchIndex = services.GetService<ITableRepository<QuoteSearchIndexTableEntity>>();
            tableRepoQuoteSearchIndex.StatusUpdate += TableRepositoryOnStatusUpdate;
            if (mode == PersistenceMode.Overwrite)
            {
                // Delete and recreate
                ConsoleInformationWriteLine("Deleting and re-creating the quote search index table repository.");
                await tableRepoQuoteSearchIndex.DeleteTableAsync();
                await tableRepoQuoteSearchIndex.CreateTableAsync();
            }

            // Quote impression table repository
            var tableRepoQuoteImpression = services.GetService<ITableRepository<QuoteImpressionTableEntity>>();
            tableRepoQuoteImpression.StatusUpdate += TableRepositoryOnStatusUpdate;
            if (mode == PersistenceMode.Overwrite)
            {
                // Delete and recreate
                ConsoleInformationWriteLine("Deleting and re-creating the quote impression table repository.");
                await tableRepoQuoteImpression.DeleteTableAsync();
                await tableRepoQuoteImpression.CreateTableAsync();
            }

            // File share storage for the original images
            var fileRepoQuoteOriginal = services.GetService<IFileShareRepository<OriginalImageFile>>();
            fileRepoQuoteOriginal.StatusUpdate += FileShareRepositoryOnStatusUpdate;
            if (mode == PersistenceMode.Overwrite)
            {
                // Delete and recreate
                ConsoleInformationWriteLine("Deleting and re-creating the file share storage for the original images.");
                await fileRepoQuoteOriginal.DeleteAsync();
                await fileRepoQuoteOriginal.CreateAsync();
                await fileRepoQuoteOriginal.CreateDirectoryAsync();
            }

            // File share storage for the resized images
            var fileRepoQuoteResized = services.GetService<IFileShareRepository<ResizedImageFile>>();
            fileRepoQuoteResized.StatusUpdate += FileShareRepositoryOnStatusUpdate;
            if (mode == PersistenceMode.Overwrite)
            {
                // Delete and recreate
                ConsoleInformationWriteLine("Deleting and re-creating the file share directory for the resized images.");
                await fileRepoQuoteResized.CreateDirectoryAsync();
            }

            // File share storage for the thumbnail images
            var fileRepoQuoteThumbnail = services.GetService<IFileShareRepository<ThumbnailImageFile>>();
            fileRepoQuoteThumbnail.StatusUpdate += FileShareRepositoryOnStatusUpdate;
            if (mode == PersistenceMode.Overwrite)
            {
                // Delete and recreate
                ConsoleInformationWriteLine("Deleting and re-creating the file share directory for the thumbnail images.");
                await fileRepoQuoteThumbnail.CreateDirectoryAsync();

                ConsoleInformationWriteLine();
            }

            // Get the quote parser
            var quoteParser = services.GetService<IQuoteParser>();
            var parsedQuotes = quoteParser.ParseInputFileAsync();
            int i = 0;
            await foreach (var parsedQuote in parsedQuotes)
            {
                i++;
                string output = $"{i}: {parsedQuote}";
                Console.WriteLine(output);
                if (parsedQuote.Errors?.Any() == true)
                {
                    ConsoleErrorWriteLine("Parsed quote errors:");
                    foreach (var parsedQuoteError in parsedQuote.Errors)
                    {
                        ConsoleErrorWriteLine(parsedQuoteError);
                    }
                }
                else
                {
                    // Increment the quote counter
                    quoteRowCounter++;

                    // Map to Azure Table entity and persist
                    var quoteTableEntity = new QuoteTableEntity()
                    {
                        FileName = parsedQuote.FileName,
                        OriginalImageHeight = parsedQuote.OriginalImageHeight,
                        OriginalImageWidth = parsedQuote.OriginalImageWidth,
                        ResizedImageHeight = parsedQuote.ResizedImageHeight,
                        ResizedImageWidth = parsedQuote.ResizedImageWidth,
                        ThumbnailImageHeight = parsedQuote.ThumbnailImageHeight,
                        ThumbnailImageWidth = parsedQuote.ThumbnailImageWidth,
                        KeyWords = parsedQuote.DelimitedKeyWords,
                        Description = parsedQuote.Description,
                        Text = parsedQuote.Text,
                        Timestamp = DateTimeOffset.UtcNow,
                        SearchIndex = parsedQuote.DelimitedSearchItems,
                        PartitionKey = "Quote",
                        RowKey = quoteRowCounter.ToString()
                    };
                    await tableRepoQuote.AddOrUpdateAsync(quoteTableEntity);

                    // Search index words. Add a table row per word. These will have the same FK row key as Quote.
                    var searchIndexWords = parsedQuote.DelimitedSearchItems?.Split(' ');
                    if (searchIndexWords != null)
                    {
                        foreach (var word in searchIndexWords)
                        {
                            var searchIndexTableEntity = new QuoteSearchIndexTableEntity()
                            {
                                PartitionKey = "QuoteSearchIndex",
                                RowKey = System.Guid.NewGuid().ToString(),
                                QuoteRowKey = quoteTableEntity.RowKey,
                                Word = word,
                                Timestamp = quoteTableEntity.Timestamp
                            };
                            await tableRepoQuoteSearchIndex.AddOrUpdateAsync(searchIndexTableEntity);
                        }
                    } // if (searchIndexWords != null)

                    // Persist original, resized, and thumbnail images to Azure file share
                    await fileRepoQuoteOriginal.AddOrUpdateAsync(new OriginalImageFile
                    {
                        Contents = parsedQuote.OriginalImage,
                        Name = parsedQuote.FileName
                    });
                    await fileRepoQuoteResized.AddOrUpdateAsync(new ResizedImageFile()
                    {
                        Contents = parsedQuote.ResizedImage,
                        Name = parsedQuote.FileName
                    });
                    await fileRepoQuoteThumbnail.AddOrUpdateAsync(new ThumbnailImageFile()
                    {
                        Contents = parsedQuote.ThumbnailImage,
                        Name = parsedQuote.FileName
                    });
                }

                ConsoleInformationWriteLine("-----");
                ConsoleInformationWriteLine();

            } // await foreach (var parsedQuote in parsedQuotes)

            // Update the total row quote count
            quoteMetadata.ETag = Azure.ETag.All;
            quoteMetadata.QuoteTotalCount = quoteRowCounter;
            quoteMetadata.Timestamp = DateTimeOffset.UtcNow;
            await tableRepoQuoteMetadata.AddOrUpdateAsync(quoteMetadata);

            ConsoleInformationWriteLine();
            ConsoleInformationWriteLine("[ Upload process has completed ]");
        }

        /// <summary>
        /// Validate that the argument is only an 'a' or 'o'.
        /// </summary>
        /// <param name="arg">Input/argument.</param>
        /// <param name="errorMessage">Error message, if return is false.</param>
        /// <param name="persistenceMode">Append, Overwrite, or Unknown when in error.</param>
        /// <returns>True if valid, false if otherwise.</returns>
        private static bool ValidateArgument(string arg, out string errorMessage, out PersistenceMode persistenceMode)
        {
            // Assume no error
            persistenceMode = PersistenceMode.Unknown;

            if (string.IsNullOrEmpty(arg) == true)
            {
                errorMessage = "Argument is missing.";
                return false;
            }

            // Assume no error
            errorMessage = null;

            string argCopy = arg;

            // Remove whitespace and lowercase it
            arg = arg.Trim().ToLowerInvariant();

            // Remove any delimiters
            if (arg.Contains('-') == true)
            {
                arg = arg.Replace("-", string.Empty);
            }
            if (arg.Contains('/') == true)
            {
                arg = arg.Replace("/", string.Empty);
            }

            // Take the first character
            arg = arg[0].ToString();

            switch (arg)
            {
                case "a":
                    {
                        persistenceMode = PersistenceMode.Append;
                        return true;
                    }
                case "o":
                    {
                        persistenceMode = PersistenceMode.Overwrite;
                        return true;
                    }
                default:
                    {
                        errorMessage = $"{argCopy} is not a valid input.";
                        return false;
                    }
            } // switch (arg)
        }

        private static ServiceProvider Configure()
        {
            var configBuilder = new ConfigurationBuilder()
                         .AddJsonFile("appsettings.json", optional: true);
            var config = configBuilder.Build();

            // Get the Azure Table storage connection string
            string tblStgConnectionString = config["Azure:StorageConnectionString"];

            // Settings for the quote flat file parser and image resizing
            int maxResizedImageDimension = System.Convert.ToInt32(config["QuoteParser:MaximumResizedImageDimension"]);
            int maxThumbnailImageDimension = System.Convert.ToInt32(config["QuoteParser:MaximumThumbnailImageDimension"]);
            string inputImagePath = config["QuoteParser:InputImagePath"];
            string quoteTextFile = config["QuoteParser:QuoteTextFile"];

            var sp = new ServiceCollection()
                .AddLogging(b => b.AddConsole())
                //.AddSingleton<IConfiguration>(config)
                .AddScoped<ITableRepository<QuoteTableEntity>, QuoteTableRepository>(f =>
                {
                    // Azure table repository for quotes
                    var tblRepo = new QuoteTableRepository(tblStgConnectionString);
                    return tblRepo;
                })
                .AddScoped<ITableRepository<QuoteSearchIndexTableEntity>, QuoteSearchIndexTableRepository>(f =>
                {
                    // Azure table repository for quote search
                    var tblRepo = new QuoteSearchIndexTableRepository(tblStgConnectionString);
                    return tblRepo;
                })
                .AddScoped<ITableRepository<QuoteMetadataTableEntity>, QuoteMetadataTableRepository>(f =>
                {
                    // Azure table repository for quote metadata
                    var tblRepo = new QuoteMetadataTableRepository(tblStgConnectionString);
                    return tblRepo;
                })
                .AddScoped<ITableRepository<QuoteImpressionTableEntity>, QuoteImpressionTableRepository>(f =>
                {
                    // Azure table repository for quote impressions
                    var tblRepo = new QuoteImpressionTableRepository(tblStgConnectionString);
                    return tblRepo;
                })
                .AddScoped<IFileShareRepository<OriginalImageFile>, OriginalQuoteFileShareRepository>(f =>
                {
                    // Azure file share repository for original quote images
                    var fileShare = new OriginalQuoteFileShareRepository(tblStgConnectionString);
                    return fileShare;
                })
                .AddScoped<IFileShareRepository<ResizedImageFile>, ResizedQuoteFileShareRepository>(f =>
                {
                    // Azure file share repository for resized quote images
                    var fileShare = new ResizedQuoteFileShareRepository(tblStgConnectionString);
                    return fileShare;
                })
                .AddScoped<IFileShareRepository<ThumbnailImageFile>, ThumbnailQuoteFileShareRepository>(f =>
                {
                    // Azure file share repository for thumbnail quote images
                    var fileShare = new ThumbnailQuoteFileShareRepository(tblStgConnectionString);
                    return fileShare;
                })
                .AddScoped(f =>
                {
                    // Quote flat file parser and image resizing
                    var quoteParserSettings = new QuoteParserSettings()
                    {
                        InputImagePath = inputImagePath,
                        MaximumResizedImageDimension = maxResizedImageDimension,
                        MaximumThumbnailImageDimension = maxThumbnailImageDimension,
                        QuoteTextFile = quoteTextFile
                    };

                    return quoteParserSettings;
                })
                .AddScoped<IQuoteService, QuoteService>()
                .AddScoped<IQuoteParser, QuoteParser>()
                .BuildServiceProvider();

            return sp;
        }

        /// <summary>
        /// Status update event handler for file share repositories.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e"><see cref="FileShareEventArgs"/></param>
        private static void FileShareRepositoryOnStatusUpdate(object sender, FileShareEventArgs e)
        {
            switch (e.Status)
            {
                case OperationStatus.Success:
                    {
                        ConsoleInformationWriteLine(e.Message);
                        break;
                    }
                case OperationStatus.Warning:
                    {
                        ConsoleWarningWriteLine(e.Message);
                        break;
                    }
                case OperationStatus.Error:
                    {
                        ConsoleErrorWriteLine(e.Message);
                        break;
                    }
            } // switch (e.Status)
        }

        /// <summary>
        /// Status update event handler for table repositories.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e"><see cref="TableRepositoryEventArgs"/></param>
        private static void TableRepositoryOnStatusUpdate(object sender, TableRepositoryEventArgs e)
        {
            switch (e.Status)
            {
                case OperationStatus.Success:
                    {
                        ConsoleInformationWriteLine(e.Message);
                        break;
                    }
                case OperationStatus.Warning:
                    {
                        ConsoleWarningWriteLine(e.Message);
                        break;
                    }
                case OperationStatus.Error:
                    {
                        ConsoleErrorWriteLine(e.Message);
                        break;
                    }
            } // switch (e.Status)
        }

        /// <summary>
        /// Set console foreground color to red and display error.
        /// </summary>
        /// <param name="value">Data to display.</param>
        private static void ConsoleErrorWriteLine(object value = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(value);
            Console.ForegroundColor = OriginalConsoleForegroundColor;
        }

        /// <summary>
        /// Set console foreground color to yellow and display warning.
        /// </summary>
        /// <param name="value">Data to display.</param>
        private static void ConsoleWarningWriteLine(object value = null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(value);
            Console.ForegroundColor = OriginalConsoleForegroundColor;
        }

        /// <summary>
        /// Set console foreground color to original and display information.
        /// </summary>
        /// <param name="value">Data to display.</param>
        private static void ConsoleInformationWriteLine(object value = null)
        {
            Console.ForegroundColor = OriginalConsoleForegroundColor;
            Console.WriteLine(value);
        }
    }
}
