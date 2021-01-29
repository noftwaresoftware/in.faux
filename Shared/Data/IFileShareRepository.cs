using Azure.Storage.Files.Shares.Models;
using Noftware.In.Faux.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Shared.Data
{
	/// <summary>
	/// Azure file share repository.
	/// </summary>
	public interface IFileShareRepository<TFile> where TFile : File, new()
	{
		// https://www.serverless360.com/blog/azure-blob-storage-vs-file-storage

		/// <summary>
		/// Delete the Azure file share.
		/// </summary>
		Task DeleteAsync();

		/// <summary>
		/// Create the Azure file share directory.
		/// </summary>
		Task CreateAsync();

		/// <summary>
		/// Delete the Azure file share directory.
		/// </summary>
		Task DeleteDirectoryAsync();

		/// <summary>
		/// Create the Azure file share directory.
		/// </summary>
		Task CreateDirectoryAsync();

		/// <summary>
		/// Add or update the file.
		/// </summary>
		/// <param name="file">File.</param>
		/// <returns><see cref="Task{ShareFileUploadInfo}"/></returns>
		Task<ShareFileUploadInfo> AddOrUpdateAsync(TFile file);

		/// <summary>
		/// Based on the filename in the existing directory, get the file contents as a byte array.
		/// </summary>
		/// <param name="fileName">Name of existing file.</param>
		/// <returns><see cref="Task{TFile}"/> or null if file not found.</returns>
		Task<TFile> GetFileAsync(string fileName);
    }
}
