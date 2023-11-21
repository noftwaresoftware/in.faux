// Ignore Spelling: Noftware Faux

namespace Noftware.In.Faux.Core.Data
{
    /// <summary>
    /// Event arguments for Azure file share notifications.
    /// </summary>
    /// <param name="message">Notification message.</param>
    /// <param name="operationStatus">Status of the operation.</param>
    public class FileShareEventArgs(string message, OperationStatus operationStatus) : DataEventArgs(message, operationStatus) { }
}
