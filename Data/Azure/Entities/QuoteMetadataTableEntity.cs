using Noftware.In.Faux.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Data.Azure.Entities
{
    /// <summary>
    /// Azure Table entity for quote a single quote meta data.
    /// </summary>
    public class QuoteMetadataTableEntity : BaseTableEntity
    {
        /// <summary>
        /// The total number of quote rows.
        /// </summary>
        public int QuoteTotalCount { get; set; }
    }
}
