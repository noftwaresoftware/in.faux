using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Core.Data
{
    /// <summary>
    /// Event arguments for Azure table repository notifications.
    /// </summary>
    public class TableRepositoryEventArgs : DataEventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Notification message.</param>
        /// <param name="operationStatus">Status of the operation.</param>
        public TableRepositoryEventArgs(string message, OperationStatus operationStatus) : base(message, operationStatus) { }
    }
}
