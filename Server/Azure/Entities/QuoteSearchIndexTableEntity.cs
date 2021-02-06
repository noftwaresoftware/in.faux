using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Server.Azure.Entities
{
	/// <summary>
	/// Azure Table entity for quote search index.
	/// </summary>
	public class QuoteSearchIndexTableEntity : TableEntity
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
