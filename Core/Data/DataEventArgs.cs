// Ignore Spelling: Noftware Faux

using System;

namespace Noftware.In.Faux.Core.Data
{
    /// <summary>
    /// Base class for all data-related event arguments.
    /// </summary>
    /// <param name="message">Notification message.</param>
    /// <param name="status">Status of the operation.</param>
    public abstract class DataEventArgs(string message, OperationStatus status) : EventArgs
    {
        /// <summary>
        /// Notification message.
        /// </summary>
        public string Message { get; private set; } = message;

        /// <summary>
        /// Status of the operation.
        /// </summary>
        public OperationStatus Status { get; set; } = status;
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
