// Ignore Spelling: Noftware Faux

using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Data.Azure.Entities;

namespace Noftware.In.Faux.Data.Repositories
{
    /// <summary>
    /// The Azure file share repository for resized images. These are the ones shown with each quote.
    /// </summary>
    /// <remarks>
    /// Constructor to set the Azure storage connection string.
    /// </remarks>
    /// <param name="storageConnectionString">Azure storage connection string.</param>
    public class ResizedQuoteFileShareRepository(string storageConnectionString) : FileShareRepository<ResizedImageFile>(storageConnectionString, "quote", "resized") { }
}
