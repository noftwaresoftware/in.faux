// Ignore Spelling: Noftware Faux

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Noftware.In.Faux.Client.Services;
using Noftware.In.Faux.Client.ViewModels;

namespace Noftware.In.Faux.Client.Pages.Quotes.Components
{
    /// <summary>
    /// Display a list of quotes with a thumbnail image.
    /// </summary>
    public partial class QuoteList
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
        /// Parameter: The list of quotes to display.
        /// </summary>
        [Parameter]
        public IEnumerable<ViewQuote> Quotes { get; set; }

        /// <summary>
        /// Icon for closed-panel.
        /// </summary>
        private const string ClosedIcon = "oi-caret-bottom";

        /// <summary>
        /// Icon for open-panel.
        /// </summary>
        private const string OpenIcon = "oi-caret-top";

        /// <summary>
        /// Table row click event handler: Display or hide the quote.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        private async Task DisplayQuoteAsync(ViewQuote quote)
        {
            // Get the display image, if it was not loaded
            if (quote.Base64Image == null)
            {
                await BusyIndicatorVisibilityAsync(quote, showBusyIndicator: true);

                string resizedImage = await this.QuoteService.GetResizedImageAsync(quote.Key, quote.FileName);
                quote.Base64Image = resizedImage;

                await BusyIndicatorVisibilityAsync(quote, showBusyIndicator: false);
            }

            // Does the 'display' image need to be fetched?
            if (string.IsNullOrEmpty(quote.SearchPanelVisibilityClass) || quote.SearchPanelVisibilityClass == ClosedIcon)
            {
                // Expand the panel to show the quote
                quote.SearchPanelVisibilityClass = OpenIcon;
                quote.SearchShowResizedImage = true;
            }
            else
            {
                // Collapse the panel to hide the quote
                quote.SearchPanelVisibilityClass = ClosedIcon;
                quote.SearchShowResizedImage = false;
            }

            // Notify the component that the state has changed
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Show or hide the busy indicator.
        /// </summary>
        /// <param name="quote">The associated quote's row to show the busy indicator within.</param>
        /// <param name="showBusyIndicator">True to show or false to hide.</param>
        /// <returns><see cref="Task"/></returns>
        private static async Task BusyIndicatorVisibilityAsync(ViewQuote quote, bool showBusyIndicator)
        {
            // Show or hide busy indicator
            quote.SearchShowBusyIndicator = showBusyIndicator;
            await Task.Delay(1);
        }
    }
}
