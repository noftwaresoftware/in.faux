// Ignore Spelling: Noftware Faux

using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Data.Azure.Entities;

namespace Noftware.In.Faux.Data.Repositories
{
    /// <summary>
    /// The Azure file share repository for thumbnail images. These are the ones shown with multiple search results.
    /// </summary>
    /// <param name="storageConnectionString">Azure storage connection string.</param>
    public class ThumbnailQuoteFileShareRepository(string storageConnectionString) : FileShareRepository<ThumbnailImageFile>(storageConnectionString, "quote", "thumbnail") { }
}
