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
    /// The Azure table repository for the single quote metadata item.
    /// </summary>
    public class QuoteMetadataTableRepository : TableRepository<QuoteMetadataTableEntity>
    {
        /// <summary>
        /// Constructor to set the Azure storage connection string.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        public QuoteMetadataTableRepository(string storageConnectionString) : base(storageConnectionString, "QuoteMetadata", "QuoteMetadata") { }
    }
}
