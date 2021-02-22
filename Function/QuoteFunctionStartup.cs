using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noftware.In.Faux.Data.Repositories;
using Noftware.In.Faux.Core.Data;
using Noftware.In.Faux.Data.Azure.Entities;
using Noftware.In.Faux.Data.Services;
using Noftware.In.Faux.Core.Services;
using Noftware.In.Faux.Data.Azure;

[assembly: FunctionsStartup(typeof(Noftware.In.Faux.Function.QuoteFunctionStartup))]

namespace Noftware.In.Faux.Function
{
    /// <summary>
    /// DI for function.
    /// </summary>
    public class QuoteFunctionStartup : FunctionsStartup
    {
        /// <summary>
        /// DI for function.
        /// </summary>
        /// <param name="builder">Function host builder.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            // Get the Azure Table storage connection string
            string tblStgConnectionString = Environment.GetEnvironmentVariable("TableStorageConnectionString");

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

            // Azure table repository for quote metadata
            services.AddScoped<ITableRepository<QuoteMetadataTableEntity>, QuoteMetadataTableRepository>(f =>
            {
                var tblRepo = new QuoteMetadataTableRepository(tblStgConnectionString);
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

            services.AddLogging();
            services.AddScoped<IQuoteService, QuoteService>();
        }
    }
}
