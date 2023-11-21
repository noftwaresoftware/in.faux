// Ignore Spelling: Noftware Faux

using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Data.Azure.Entities;

namespace Noftware.In.Faux.Data.Repositories
{
    /// <summary>
    /// The Azure table repository for quotes.
    /// </summary>
    /// <param name="storageConnectionString">Azure storage connection string.</param>
    public class QuoteTableRepository(string storageConnectionString) : TableRepository<QuoteTableEntity>(storageConnectionString, "Quote", "Quote") { }
}
