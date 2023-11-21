// Ignore Spelling: Noftware Faux Metadata

using Noftware.In.Faux.Core.Models;

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
