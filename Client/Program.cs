using Noftware.In.Faux.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Noftware.In.Faux.Client.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Inject the client quote service
            builder.Services.AddScoped<IClientQuoteService, ClientQuoteService>();

            await builder.Build().RunAsync();
        }
    }
}
