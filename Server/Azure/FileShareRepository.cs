using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Noftware.In.Faux.Shared.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Server.Azure
{
    /// <summary>
    /// Azure file share repository.
    /// </summary>
    public abstract class FileShareRepository<TFile> : IFileShareRepository<TFile> where TFile : Shared.Models.File, new()
    {
        // Connection
        private readonly ShareClient _shareClient;

        // Directory/folder
        private readonly ShareDirectoryClient _shareDirectory;

        // Azure file share name
        private readonly string _shareName;

        // Directory/folder within the file share
        private readonly string _directoryName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        /// <param name="shareName">Azure file share name.</param>
        /// <param name="directoryName">Directory/folder within the file share.</param>
        public FileShareRepository(string storageConnectionString, string shareName, string directoryName)
        {
            _shareName = shareName;
            _directoryName = directoryName;

            // Create 'file share' if it does not exist
            _shareClient = new ShareClient(storageConnectionString, shareName);
            _shareClient.CreateIfNotExists();

            _shareDirectory = _shareClient.GetDirectoryClient(directoryName);
            _shareDirectory.CreateIfNotExists();
        }

        /// <summary>
        /// Delete the Azure file share.
        /// </summary>
        public async Task DeleteAsync()
        {
            await _shareClient.DeleteAsync();
        }

        /// <summary>
        /// Create the Azure file share directory.
        /// </summary>
        public async Task CreateAsync()
        {
            const int MaxTries = 60;
            int count = 0;
            do
            {
                try
                {
                    await _shareClient.CreateIfNotExistsAsync();
                    count = MaxTries + 1;
                }
                catch (RequestFailedException)
                {
                    Console.WriteLine($"{count + 1}: The '{_shareName}' share is currently being deleted. Please wait.");
                    await Task.Delay(1000); // The share is currently being deleted. Try again until it works.
                }

                count++;

            } while (count < MaxTries);
        }

        /// <summary>
        /// Delete the Azure file share directory.
        /// </summary>
        public async Task DeleteDirectoryAsync()
        {
            await _shareDirectory.DeleteAsync();
        }

        /// <summary>
        /// Create the Azure file share directory.
        /// </summary>
        public async Task CreateDirectoryAsync()
        {
            const int MaxTries = 60;
            int count = 0;
            do
            {
                try
                {
                    await _shareDirectory.CreateIfNotExistsAsync();
                    count = MaxTries + 1;
                }
                catch (RequestFailedException)
                {
                    Console.WriteLine($"{count + 1}: The '{_directoryName}' directory is currently being deleted. Please wait.");
                    await Task.Delay(1000); // The directory is currently being deleted. Try again until it works.
                }

                count++;

            } while (count < MaxTries);
        }

        /// <summary>
        /// Add or update the file.
        /// </summary>
        /// <param name="file">File.</param>
        /// <returns><see cref="ShareFileUploadInfo"/></returns>
        public async Task<ShareFileUploadInfo> AddOrUpdateAsync(TFile file)
        {
            //  Azure allows for 4MB max uploads  (4 x 1024 x 1024 = 4194304)
            const int UploadLimit = 4194304;

            ShareFileUploadInfo result = null;

            var stream = new MemoryStream(file.Contents);

            stream.Seek(0, SeekOrigin.Begin);   // ensure stream is at the beginning
            var fileClient = await _shareDirectory.CreateFileAsync(file.Name, stream.Length);

            // If stream is below the limit upload directly
            if (stream.Length <= UploadLimit)
            {
                result = await fileClient.Value.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
                return result;
            }

            int bytesRead;
            long index = 0;
            byte[] buffer = new byte[UploadLimit];

            // Stream is larger than the limit so we need to upload in chunks
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                // Create a memory stream for the buffer to upload
                using MemoryStream ms = new MemoryStream(buffer, 0, bytesRead);
                result = await fileClient.Value.UploadRangeAsync(new HttpRange(index, ms.Length), ms);
                index += ms.Length; // increment the index to the account for bytes already written
            }

            return result;
        }

        /// <summary>
        /// Based on the filename in the existing directory, get the file contents as a byte array.
        /// </summary>
        /// <param name="fileName">Name of existing file.</param>
        /// <returns><see cref="Task{TFile}"/> or null if file not found.</returns>
        public async Task<TFile> GetFileAsync(string fileName)
        {
            var file = new TFile()
            {
                Name = fileName
            };

            try
            {
                var fileClient = _shareDirectory.GetFileClient(fileName);
                if (await fileClient.ExistsAsync() == true)
                {
                    using (var stream = await fileClient.OpenReadAsync(new ShareFileOpenReadOptions(false)))
                    {
                        file.Name = fileClient.Name;
                        file.Path = fileClient.Path;

                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            file.Contents = memoryStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return file;
        }
    }
}
