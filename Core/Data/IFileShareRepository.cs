// Ignore Spelling: Noftware Faux

using System;
using System.Threading.Tasks;
using Noftware.In.Faux.Core.Models;

namespace Noftware.In.Faux.Core.Data
{
    /// <summary>
    /// Azure file share repository.
    /// </summary>
    public interface IFileShareRepository<TFile> where TFile : File, new()
	{
		// https://www.serverless360.com/blog/azure-blob-storage-vs-file-storage

		/// <summary>
		/// Event handler to notify caller of status.
		/// </summary>
		event EventHandler<FileShareEventArgs> StatusUpdate;

		/// <summary>
		/// Delete the Azure file share.
		/// </summary>
		/// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
		Task<bool> DeleteAsync();

		/// <summary>
		/// Create the Azure file share directory.
		/// </summary>
		/// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
		Task<bool> CreateAsync();

		/// <summary>
		/// Delete the Azure file share directory.
		/// </summary>
		/// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
		Task<bool> DeleteDirectoryAsync();

		/// <summary>
		/// Create the Azure file share directory.
		/// </summary>
		/// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
		Task<bool> CreateDirectoryAsync();

		/// <summary>
		/// Add or update the file.
		/// </summary>
		/// <param name="file">File.</param>
		/// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
		Task<bool> AddOrUpdateAsync(TFile file);

		/// <summary>
		/// Based on the filename in the existing directory, get the file contents as a byte array.
		/// </summary>
		/// <param name="fileName">Name of existing file.</param>
		/// <returns><see cref="File"/> or null if file not found.</returns>
		Task<TFile> GetFileAsync(string fileName);
    }
}
