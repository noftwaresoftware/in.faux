// Ignore Spelling: Noftware Faux

namespace Noftware.In.Faux.Core.Data
{
    /// <summary>
    /// Event arguments for Azure table repository notifications.
    /// </summary>
    /// <param name="message">Notification message.</param>
    /// <param name="operationStatus">Status of the operation.</param>
    public class TableRepositoryEventArgs(string message, OperationStatus operationStatus) : DataEventArgs(message, operationStatus)     {     }
}
