// Ignore Spelling: Noftware Faux

using Noftware.In.Faux.Core.Models;

namespace Noftware.In.Faux.Data.Azure.Entities
{
    /// <summary>
    /// Represents a single view/impression of a quote.
    /// </summary>
    public class QuoteImpressionTableEntity : BaseTableEntity
    {
        /// <summary>
        /// The row key (int) of the quote.
        /// </summary>
        public string QuoteRowKey { get; set; }
    }
}
