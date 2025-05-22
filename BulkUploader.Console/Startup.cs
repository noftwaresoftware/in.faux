// Ignore Spelling: Noftware Faux Uploader

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    /// DI startup.
    /// </summary>
    /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
    public class Startup(IConfiguration configuration)
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        public IConfiguration Configuration { get; } = configuration;

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services">Specifies the contract for a collection of service descriptors.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            // Get the Azure Table storage connection string
            string tblStgConnectionString = this.Configuration["Azure:StorageConnectionString"];

            // Azure table repository for quotes
            services.AddScoped<ITableRepository<QuoteTableEntity>, QuoteTableRepository>(f =>
            {
                var tblRepo = new QuoteTableRepository(tblStgConnectionString);
                return tblRepo;
            });

            // Azure table repository for quote search
            services.AddScoped<ITableRepository<QuoteSearchIndexTableEntity>, QuoteSearchIndexTableRepository>(f =>
            {
                var tblRepo = new QuoteSearchIndexTableRepository(tblStgConnectionString);
                return tblRepo;
            });

            // Azure table repository for quote impressions
            services.AddScoped<ITableRepository<QuoteImpressionTableEntity>, QuoteImpressionTableRepository>(f =>
            {
                var tblRepo = new QuoteImpressionTableRepository(tblStgConnectionString);
                return tblRepo;
            });

            // Azure file share repository for resized quote images
            services.AddScoped<IFileShareRepository<ResizedImageFile>, ResizedQuoteFileShareRepository>(f =>
            {
                var fileShare = new ResizedQuoteFileShareRepository(tblStgConnectionString);
                return fileShare;
            });

            // Azure file share repository for thumbnail quote images
            services.AddScoped<IFileShareRepository<ThumbnailImageFile>, ThumbnailQuoteFileShareRepository>(f =>
            {
                var fileShare = new ThumbnailQuoteFileShareRepository(tblStgConnectionString);
                return fileShare;
            });

            // Settings for the quote flat file parser and image resizing
            int maxResizedImageDimension = System.Convert.ToInt32(this.Configuration["QuoteParser:MaximumResizedImageDimension"]);
            int maxThumbnailImageDimension = System.Convert.ToInt32(this.Configuration["QuoteParser:MaximumThumbnailImageDimension"]);
            string inputImagePath = this.Configuration["QuoteParser:InputImagePath"];
            string quoteTextFile = this.Configuration["QuoteParser:QuoteTextFile"];
            services.AddScoped(f =>
            {
                var quoteParserSettings = new QuoteParserSettings()
                {
                    InputImagePath = inputImagePath,
                    MaximumResizedImageDimension = maxResizedImageDimension,
                    MaximumThumbnailImageDimension = maxThumbnailImageDimension,
                    QuoteTextFile = quoteTextFile
                };

                return quoteParserSettings;
            });

            services.AddScoped<IQuoteService, QuoteService>();
            services.AddScoped<IQuoteParser, QuoteParser>();
        }
    }
}
