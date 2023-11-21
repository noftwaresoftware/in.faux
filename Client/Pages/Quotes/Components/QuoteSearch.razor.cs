// Ignore Spelling: Noftware Faux

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Noftware.In.Faux.Client.Services;
using Noftware.In.Faux.Client.ViewModels;
using Noftware.In.Faux.Core.Extensions;

namespace Noftware.In.Faux.Client.Pages.Quotes.Components
{
    /// <summary>
    /// Search functionality to allow the user to search for quotes based on the search index.
    /// </summary>
    public partial class QuoteSearch
    {
        /// <summary>
        /// Injected Quote Service.
        /// </summary>
        [Inject]
        public IClientQuoteService QuoteService { get; set; }

        /// <summary>
        /// Injected Configuration.
        /// </summary>
        [Inject]
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// Show busy indicator?
        /// </summary>
        private bool ShowBusyIndicator { get; set; }

        /// <summary>
        /// Quote search view model.
        /// </summary>
        private readonly ViewQuoteSearch QuoteSearchView = new();

        /// <summary>
        /// Results to display.
        /// </summary>
        private IEnumerable<ViewQuote> SearchResults = null;

        /// <summary>
        /// True if no search results were found based on the search phrase.
        /// </summary>
        private bool NoResultsFound = false;

        /// <summary>
        /// True if search results were found based on the search phrase.
        /// </summary>
        private bool ResultsFound = false;

        /// <summary>
        /// Count of the search results for display.
        /// </summary>
        private string TextName;

        /// <summary>
        /// Name of the quote's text field (for display purposes).
        /// </summary>
        private string ResultsCountText;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            this.TextName = Configuration["TextName"];
        }

        /// <summary>
        /// Enter key: Perform search based on space separated words.
        /// </summary>
        private async Task OnSearchKeyUpAsync(KeyboardEventArgs e)
        {
            if (e.Code == "Enter" || e.Code == "NumpadEnter")
            {
                await OnSubmitAsync();
            }
        }

        /// <summary>
        /// Search button: Perform search based on space separated words.
        /// </summary>
        private async Task OnSubmitAsync()
        {
            this.NoResultsFound = false;
            this.SearchResults = null;
            this.ResultsCountText = null;

            // Show busy indicator
            await this.BusyIndicatorVisibilityAsync(showBusyIndicator: true);

            this.SearchResults = await this.QuoteService.SearchQuotesAsync(QuoteSearchView.Phrase);

            // Show the 'not found' or 'results' panel
            this.NoResultsFound = (SearchResults.Any() == false);
            this.ResultsFound = !NoResultsFound;

            // Get the results count. Only shown if ResultsFound is true.
            this.ResultsCountText = "result".Pluralize(SearchResults.Count());

            // Hide busy indicator
            await this.BusyIndicatorVisibilityAsync(showBusyIndicator: false);

            // Notify the component that the state has changed
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Show or hide the busy indicator.
        /// </summary>
        /// <param name="showBusyIndicator">True to show or false to hide.</param>
        /// <returns><see cref="Task"/></returns>
        private async Task BusyIndicatorVisibilityAsync(bool showBusyIndicator)
        {
            // Show or hide busy indicator
            this.ShowBusyIndicator = showBusyIndicator;
            await Task.Delay(1);
        }
    }
}
