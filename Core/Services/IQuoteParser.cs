// Ignore Spelling: Noftware Faux

using System.Collections.Generic;
using Noftware.In.Faux.Core.Models;

namespace Noftware.In.Faux.Core.Services
{
    /// <summary>
    /// Parses a text file that contains quote data.
    /// Format: Quote text {Meaning/explanation text} [FileName] |Comma-separated keywords/tags|
    /// Example: Circle back {Following up on progress.} [CircleBack.png] |reach,contact,follow,up,follow-up,communicate|
    /// </summary>
    public interface IQuoteParser
	{
		/// <summary>
		/// Parses the quote text file.
		/// </summary>
		/// <returns><see cref="IAsyncEnumerable{ParsedQuote}"/></returns>
		IAsyncEnumerable<ParsedQuote> ParseInputFileAsync();
	}
}
