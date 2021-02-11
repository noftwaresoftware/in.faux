using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Data.Azure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Data.Repositories
{
    /// <summary>
    /// The Azure file share repository for thumbnail images. These are the ones shown with multiple search results.
    /// </summary>
    public class ThumbnailQuoteFileShareRepository : FileShareRepository<ThumbnailImageFile>
    {
        /// <summary>
        /// Constructor to set the Azure storage connection string.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        public ThumbnailQuoteFileShareRepository(string storageConnectionString) : base(storageConnectionString, "quote", "thumbnail") { }
    }
}
