using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using Noftware.In.Faux.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Noftware.In.Faux.Shared.Services;
using Microsoft.Extensions.Configuration;
using Noftware.In.Faux.Client.Services;
using Noftware.In.Faux.Client.ViewModels;

namespace Noftware.In.Faux.Client.Pages.Quotes.Components
{
    /// <summary>
    /// Using a button click, get a random quote.
    /// </summary>
    public partial class QuoteButton
    {
        /// <summary>
        /// Injected Configuration.
        /// </summary>
        [Inject]
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Injected Quote Service.
        /// </summary>
        [Inject]
        public IClientQuoteService QuoteService { get; set; }

        // Random quote
        private ViewQuote _quote = null;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters from its parent in the render tree.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        protected override async Task OnInitializedAsync()
        {
            // Get a random quote
            await this.GetRandomQuote();
        }

        /// <summary>
        /// Get Random button: Get a random quote.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        private async Task GetRandomOnClickAsync()
        {
            // Get a random quote
            await this.GetRandomQuote();

            // Notify the component that the state has changed
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Button click event handler to get a random quote.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        private async Task GetRandomQuote()
        {
            // For the busy indicator
            _quote = null;
            await Task.Delay(1);

            // Get a random quote
            _quote = await this.QuoteService.GetRandomQuoteAsync();

            await Task.Delay(1);
        }
    }
}
