using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using Noftware.In.Faux.Core.Models;
using Microsoft.AspNetCore.Components;
using Noftware.In.Faux.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Noftware.In.Faux.Client.Services;
using Noftware.In.Faux.Client.ViewModels;
using Microsoft.AspNetCore.Components.Web;

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
        private bool _showBusyIndicator { get; set; }

        /// <summary>
        /// Quote search view model.
        /// </summary>
        private readonly ViewQuoteSearch _quoteSearch = new ViewQuoteSearch();

        /// <summary>
        /// Results to display.
        /// </summary>
        private IEnumerable<ViewQuote> _searchResults = null;

        /// <summary>
        /// True if no search results were found based on the search phrase.
        /// </summary>
        private bool _noResultsFound = false;

        /// <summary>
        /// True if search results were found based on the search phrase.
        /// </summary>
        private bool _resultsFound = false;

        /// <summary>
        /// Count of the search results for display.
        /// </summary>
        private string _textName;

        /// <summary>
        /// Name of the quote's text field (for display purposes).
        /// </summary>
        private string _resultsCountText;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            _textName = Configuration["TextName"];
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
            _noResultsFound = false;
            _searchResults = null;
            _resultsCountText = null;

            // Show busy indicator
            await this.BusyIndicatorVisibilityAsync(showBusyIndicator: true);

            _searchResults = await this.QuoteService.SearchQuotesAsync(_quoteSearch.Phrase);

            // Show the 'not found' or 'results' panel
            _noResultsFound = (_searchResults.Any() == false);
            _resultsFound = !_noResultsFound;

            // Get the results count. Only shown if _resultsFound is true.
            _resultsCountText = "result".Pluralize(_searchResults.Count());

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
            _showBusyIndicator = showBusyIndicator;
            await Task.Delay(1);
        }
    }
}
