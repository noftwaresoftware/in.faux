// Ignore Spelling: Noftware Faux

using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Data.Azure.Entities;

namespace Noftware.In.Faux.Data.Repositories
{
    /// <summary>
    /// The Azure file share repository for original images. These are the original/untouched images from the source directory.
    /// </summary>
    /// <remarks>
    /// Constructor to set the Azure storage connection string.
    /// </remarks>
    /// <param name="storageConnectionString">Azure storage connection string.</param>
    public class OriginalQuoteFileShareRepository(string storageConnectionString) : FileShareRepository<OriginalImageFile>(storageConnectionString, "quote", "original") { }
}
