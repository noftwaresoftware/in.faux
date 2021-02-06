using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Shared.Data
{
    /// <summary>
    /// Event arguments for Azure file share notifications.
    /// </summary>
    public class FileShareEventArgs : DataEventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Notification message.</param>
        /// <param name="operationStatus">Status of the operation.</param>
        public FileShareEventArgs(string message, OperationStatus operationStatus) : base(message, operationStatus) { }
    }
}
