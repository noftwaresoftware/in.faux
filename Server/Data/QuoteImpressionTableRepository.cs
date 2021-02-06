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
    /// The Azure table repository for the quote search index.
    /// </summary>
    public class QuoteImpressionTableRepository : TableRepository<QuoteImpressionTableEntity>
    {
        /// <summary>
        /// Constructor to set the Azure storage connection string.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        public QuoteImpressionTableRepository(string storageConnectionString) : base(storageConnectionString, "QuoteImpression", "QuoteImpression") { }
    }
}
