using Noftware.In.Faux.Server.Azure;
using Noftware.In.Faux.Server.Azure.Entities;
using Noftware.In.Faux.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Server.Data
{
    /// <summary>
    /// The Azure table repository for quotes.
    /// </summary>
    public class QuoteTableRepository : TableRepository<QuoteTableEntity>
    {
        /// <summary>
        /// Constructor to set the Azure storage connection string.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        public QuoteTableRepository(string storageConnectionString) : base(storageConnectionString, "Quote", "Quote") { }
    }
}
