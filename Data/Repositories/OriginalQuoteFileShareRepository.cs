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
    /// The Azure file share repository for original images. These are the original/untouched images from the source directory.
    /// </summary>
    public class OriginalQuoteFileShareRepository : FileShareRepository<OriginalImageFile>
    {
        /// <summary>
        /// Constructor to set the Azure storage connection string.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        public OriginalQuoteFileShareRepository(string storageConnectionString) : base(storageConnectionString, "quote", "original") { }
    }
}
