// Ignore Spelling: Noftware Faux

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Noftware.In.Faux.Client.Services;

namespace Noftware.In.Faux.Client
{
    /// <summary>
    /// Entry point for WebAssembly app.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for WebAssembly app.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns><see cref="Task"/></returns>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            string baseAddress = builder.Configuration["ApiBaseAddress"] ?? builder.HostEnvironment.BaseAddress;
            builder.Services.AddScoped(sp => new HttpClient() { BaseAddress = new Uri(baseAddress) });

            // Inject the client quote service
            builder.Services.AddScoped<IClientQuoteService, ClientQuoteService>();

            await builder.Build().RunAsync();
        }
    }
}
