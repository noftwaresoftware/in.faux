// Ignore Spelling: Noftware Faux

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Noftware.In.Faux.Client.Pages.Quotes
{
    /// <summary>
    /// Get a random quote every 'x' seconds via a timer job.
    /// </summary>
    public partial class Timer
    {
        /// <summary>
        /// Injected Configuration.
        /// </summary>
        [Inject]
        public IConfiguration Configuration { get; set; }

        // Refresh the timer every 'x' seconds
        private int SecondsRefresh = 10;

        /// <summary>
        /// Method invoked when the component is ready to start, having received its initial parameters from its parent in the render tree.
        /// </summary>
        protected override void OnInitialized()
        {
            _ = int.TryParse(Configuration["TimerRefreshSeconds"], out SecondsRefresh);
        }
    }
}
