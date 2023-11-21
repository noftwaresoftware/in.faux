// Ignore Spelling: Noftware Faux

using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Data.Azure.Entities;

namespace Noftware.In.Faux.Data.Repositories
{
    /// <summary>
    /// The Azure table repository for the quote impressions.
    /// </summary>
    /// <remarks>
    /// Constructor to set the Azure storage connection string.
    /// </remarks>
    /// <param name="storageConnectionString">Azure storage connection string.</param>
    public class QuoteImpressionTableRepository(string storageConnectionString) : TableRepository<QuoteImpressionTableEntity>(storageConnectionString, "QuoteImpression", "QuoteImpression") { }
}
