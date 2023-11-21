// Ignore Spelling: Noftware Faux Metadata

using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Data.Azure.Entities;

namespace Noftware.In.Faux.Data.Repositories
{
    /// <summary>
    /// The Azure table repository for the single quote metadata item.
    /// </summary>
    /// <remarks>
    /// Constructor to set the Azure storage connection string.
    /// </remarks>
    /// <param name="storageConnectionString">Azure storage connection string.</param>
    public class QuoteMetadataTableRepository(string storageConnectionString) : TableRepository<QuoteMetadataTableEntity>(storageConnectionString, "QuoteMetadata", "QuoteMetadata") { }
}
