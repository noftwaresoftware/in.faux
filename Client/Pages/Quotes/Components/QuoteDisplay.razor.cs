using Microsoft.AspNetCore.Components;
using Noftware.In.Faux.Client.ViewModels;
using Noftware.In.Faux.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Client.Pages.Quotes.Components
{
    /// <summary>
    /// Display the quote's data.
    /// </summary>
    public partial class QuoteDisplay
    {
        /// <summary>
        /// Parameter: The quote to display.
        /// </summary>
        [Parameter]
        public ViewQuote Quote { get; set; }
    }
}
