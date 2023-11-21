// Ignore Spelling: Noftware Faux Uploader

namespace Noftware.In.Faux.BulkUploader
{
    /// <summary>
    /// Persistence mode for quote data and images.
    /// </summary>
    public enum PersistenceMode
    {
        /// <summary>
        /// Mode not known.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Append to existing table storage and file share.
        /// </summary>
        Append = 1,

        /// <summary>
        /// Delete table repositories and file shares and re-upload all data.
        /// </summary>
        Overwrite = 2
    }
}
