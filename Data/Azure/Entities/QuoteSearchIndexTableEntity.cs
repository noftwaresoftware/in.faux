// Ignore Spelling: Noftware Faux

using Noftware.In.Faux.Core.Models;

namespace Noftware.In.Faux.Data.Azure.Entities
{
    /// <summary>
    /// Azure Table entity for quote search index.
    /// </summary>
    public class QuoteSearchIndexTableEntity : BaseTableEntity
    {
		/// <summary>
		/// The indexed quote word.
		/// </summary>
		public string Word { get; set; }

		/// <summary>
		/// The row key (int) of the quote.
		/// </summary>
		public string QuoteRowKey { get; set; }
	}
}
