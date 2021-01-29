using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Client.ViewModels
{
    /// <summary>
    /// View model for quote searching.
    /// </summary>
    public class ViewQuoteSearch
    {
        /// <summary>
        /// Search phrase. Words are delimited by spaces.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Phrase { get; set; }
    }
}
