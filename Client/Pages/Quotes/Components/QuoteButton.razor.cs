// Ignore Spelling: Noftware Faux

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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

        /// <summary>
        /// Random quote.
        /// </summary>
        private ViewQuote Quote = null;

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
            this.Quote = null;
            await Task.Delay(1);

            // Get a random quote
            this.Quote = await this.QuoteService.GetRandomQuoteAsync();

            await Task.Delay(1);
        }
    }
}
