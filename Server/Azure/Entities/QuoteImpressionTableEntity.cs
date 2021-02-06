using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Server.Azure.Entities
{
    /// <summary>
    /// Represents a single view/impression of a quote.
    /// </summary>
    public class QuoteImpressionTableEntity : TableEntity
    {
        /// <summary>
        /// The row key (int) of the quote.
        /// </summary>
        public string QuoteRowKey { get; set; }
    }
}
