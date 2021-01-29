using Noftware.In.Faux.Server.Azure;
using Noftware.In.Faux.Server.Azure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Server.Data
{
    /// <summary>
    /// The Azure file share repository for resized images. This are the ones shown with each quote.
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
