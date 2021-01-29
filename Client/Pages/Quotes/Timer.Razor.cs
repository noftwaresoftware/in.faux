using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int.TryParse(Configuration["TimerRefreshSeconds"], out SecondsRefresh);
        }
    }
}
