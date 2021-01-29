using Microsoft.AspNetCore.Components;
using Noftware.In.Faux.Client.Services;
using Noftware.In.Faux.Client.ViewModels;
using Noftware.In.Faux.Shared.Models;
using Noftware.In.Faux.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Client.Pages.Quotes.Components
{
    /// <summary>
    /// Show a single random quote (not a timer or a button).
    /// </summary>
    public partial class QuoteRandom
    {
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
            // For busy indicator
            _quote = null;
            await Task.Delay(1);

            // Get a random quote
            _quote = await this.QuoteService.GetRandomQuoteAsync();

            await Task.Delay(1);
        }
    }
}
