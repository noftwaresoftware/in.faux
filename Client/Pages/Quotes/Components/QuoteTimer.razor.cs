using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using Noftware.In.Faux.Core.Models;
using Microsoft.AspNetCore.Components;
using Noftware.In.Faux.Core.Services;
using Microsoft.Extensions.Configuration;
using Noftware.In.Faux.Client.Services;
using Noftware.In.Faux.Client.ViewModels;

namespace Noftware.In.Faux.Client.Pages.Quotes.Components
{
    /// <summary>
    /// Using a timer, display a random quote.
    /// </summary>
    public partial class QuoteTimer : IDisposable
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
        /// Parameter: The number of seconds to elapse to get the next random quote.
        /// </summary>
        [Parameter]
        public int TimerRefreshSeconds { get; set; }

        // Random quote
        private ViewQuote _quote = null;

        // Current counter for the user to see the next refresh
        private int _currentCounter = 0;

        // UI for the current counter
        private string _currentCounterDisplay = null;

        // Timer: Recurring timer to display a new quote
        private System.Timers.Timer _recurringTimer;

        // Text name
        private string _textName;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters from its parent in the render tree.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        protected override async Task OnInitializedAsync()
        {
            // Item name to display when the timer reaches zero
            _textName = Configuration["TextName"].ToLowerInvariant();

            _quote = null;
            await Task.Delay(1);

            // Run the Quote timer every 1 second
            _recurringTimer = new System.Timers.Timer(1000);
            _recurringTimer.Elapsed += this.OnElapsedQuoteTimerAsync;
            _recurringTimer.AutoReset = false;
            _recurringTimer.Start();

            await Task.Delay(1);
        }

        /// <summary>
        /// Recurring timer to display a new quote.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An System.Timers.ElapsedEventArgs object that contains the event data.</param>
        private async void OnElapsedQuoteTimerAsync(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (_currentCounter > 0)
                {
                    // Display
                    if (_currentCounter == 1)
                    {
                        // For the busy indicator
                        _currentCounterDisplay = $"Getting the next {_textName}";
                        _quote = null;
                        await Task.Delay(1);
                    }
                    else
                    {
                        _currentCounterDisplay = $"Refreshing in {_currentCounter} seconds";
                    }
                    _currentCounter--;
                }
                else
                {
                    // For the busy indicator
                    _quote = null;
                    await Task.Delay(1);

                    _currentCounterDisplay = string.Empty;

                    // Get a random quote
                    _quote = await this.QuoteService.GetRandomQuoteAsync();

                    // Reset the visual counter
                    _currentCounter = TimerRefreshSeconds;

                    await Task.Delay(1);
                }

                // Notify the component that the state has changed
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"QuoteTimer.OnTimerGetQuoteCallBack error: {ex}");
            }

            _recurringTimer.Start();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _recurringTimer?.Dispose();
        }
    }
}
