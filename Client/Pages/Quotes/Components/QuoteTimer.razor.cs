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

        /// <summary>
        /// Random quote.
        /// </summary>
        private ViewQuote Quote = null;

        /// <summary>
        /// Current counter for the user to see the next refresh.
        /// </summary>
        private int CurrentCounter = 0;

        /// <summary>
        /// UI for the current counter.
        /// </summary>
        private string CurrentCounterDisplay = null;

        /// <summary>
        /// Timer: Recurring timer to display a new quote.
        /// </summary>
        private System.Timers.Timer RecurringTimer;

        /// <summary>
        /// Text name.
        /// </summary>
        private string TextName;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters from its parent in the render tree.
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        protected override async Task OnInitializedAsync()
        {
            // Item name to display when the timer reaches zero
            this.TextName = Configuration["TextName"].ToLowerInvariant();

            Quote = null;
            await Task.Delay(1);

            // Run the Quote timer every 1 second
            this.RecurringTimer = new System.Timers.Timer(1000);
            this.RecurringTimer.Elapsed += this.OnElapsedQuoteTimerAsync;
            this.RecurringTimer.AutoReset = false;
            this.RecurringTimer.Start();

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
                if (this.CurrentCounter > 0)
                {
                    // Display
                    if (this.CurrentCounter == 1)
                    {
                        // For the busy indicator
                        this.CurrentCounterDisplay = $"Getting the next {TextName}";
                        this.Quote = null;
                        await Task.Delay(1);
                    }
                    else
                    {
                        this.CurrentCounterDisplay = $"Refreshing in {CurrentCounter} seconds";
                    }
                    this.CurrentCounter--;
                }
                else
                {
                    // For the busy indicator
                    this.Quote = null;
                    await Task.Delay(1);

                    this.CurrentCounterDisplay = string.Empty;

                    // Get a random quote
                    this.Quote = await this.QuoteService.GetRandomQuoteAsync();

                    // Reset the visual counter
                    this.CurrentCounter = TimerRefreshSeconds;

                    await Task.Delay(1);
                }

                // Notify the component that the state has changed
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"QuoteTimer.OnTimerGetQuoteCallBack error: {ex}");
            }

            this.RecurringTimer.Start();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            try
            {
                this.RecurringTimer?.Stop();
                this.RecurringTimer?.Dispose();
            }
            catch (Exception)
            {
                // Swallow exception
                Console.WriteLine("A dispose error occurred.");
            }
        }
    }
}
