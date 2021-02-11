using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Core.Data
{
    /// <summary>
    /// Base class for all data-related event arguments.
    /// </summary>
    public abstract class DataEventArgs : EventArgs
    {
        /// <summary>
        /// Notification message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Status of the operation.
        /// </summary>
        public OperationStatus Status { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Notification message.</param>
        /// <param name="status">Status of the operation.</param>
        public DataEventArgs(string message, OperationStatus status)
        {
            this.Status = status;
            this.Message = message;
        }
    }

    /// <summary>
    /// Status of the operation.
    /// </summary>
    public enum OperationStatus
    {
        /// <summary>
        /// Success.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Warning.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// An error occurred.
        /// </summary>
        Error = 2
    }
}
