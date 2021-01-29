using Noftware.In.Faux.Shared.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Noftware.In.Faux.Server.Azure;
using System.Configuration;
using System.Linq;
using Noftware.In.Faux.Server.Services;
using Noftware.In.Faux.Shared.Models;
using System;
using Noftware.In.Faux.Client.Services;
using Noftware.In.Faux.Server.Data;
using Noftware.In.Faux.Shared.Data;
using Noftware.In.Faux.Server.Azure.Entities;
using Microsoft.Extensions.Logging;

namespace Noftware.In.Faux.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Key Vault settings
            string kvUrl = this.Configuration["KeyVault:VaultUri"];
            string kvClientId = this.Configuration["KeyVault:ClientId"];
            string kvSecret = this.Configuration["KeyVault:Secret"];
            string kvTenantId = this.Configuration["KeyVault:TenantId"];
            var kvSettings = new KeyVaultSettings(kvUrl, kvTenantId, kvClientId, kvSecret);

            // Get the Azure Table storage connection string
            string tblStgConnectionString = kvSettings.GetSecret("table-storage-connection-string");

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

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
