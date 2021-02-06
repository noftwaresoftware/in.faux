using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Client.ViewModels
{
    /// <summary>
    /// Name of quote filename.
    /// </summary>
    public class ViewQuoteFileName
    {
        /// <summary>
        /// Quote's unique row key.
        /// </summary>
        public string QuoteRowKey { get; set; }

        /// <summary>
        /// Quote filename.
        /// </summary>
        public string FileName { get; set; }
    }
}
