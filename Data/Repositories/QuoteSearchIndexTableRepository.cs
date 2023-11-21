// Ignore Spelling: Noftware Faux

using Noftware.In.Faux.Data.Azure;
using Noftware.In.Faux.Data.Azure.Entities;
using Noftware.In.Faux.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Data.Repositories
{
    /// <summary>
    /// The Azure table repository for the quote search index.
    /// </summary>
    /// <param name="storageConnectionString">Azure storage connection string.</param>
    public class QuoteSearchIndexTableRepository(string storageConnectionString) : TableRepository<QuoteSearchIndexTableEntity>(storageConnectionString, "QuoteSearchIndex", "QuoteSearchIndex") { }
}
