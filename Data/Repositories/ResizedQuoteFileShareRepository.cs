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
    /// The Azure file share repository for resized images. These are the ones shown with each quote.
    /// </summary>
    public class ResizedQuoteFileShareRepository : FileShareRepository<ResizedImageFile>
    {
        /// <summary>
        /// Constructor to set the Azure storage connection string.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        public ResizedQuoteFileShareRepository(string storageConnectionString) : base(storageConnectionString, "quote", "resized") { }
    }
}
