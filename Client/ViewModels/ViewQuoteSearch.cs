// Ignore Spelling: Noftware Faux

using System.ComponentModel.DataAnnotations;

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
