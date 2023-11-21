// Ignore Spelling: Noftware Faux

using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Noftware.In.Faux.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Data.Azure
{
    /// <summary>
    /// Azure file share repository.
    /// </summary>
    public abstract class FileShareRepository<TFile> : IFileShareRepository<TFile> where TFile : Core.Models.File, new()
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
        /// Event handler to notify caller of status.
        /// </summary>
        public event EventHandler<FileShareEventArgs> StatusUpdate;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="storageConnectionString">Azure storage connection string.</param>
        /// <param name="shareName">Azure file share name.</param>
        /// <param name="directoryName">Directory/folder within the file share.</param>
        public FileShareRepository(string storageConnectionString, string shareName, string directoryName)
        {
            _shareName = shareName.ToLowerInvariant();
            _directoryName = directoryName;

            // Create 'file share' if it does not exist
            _shareClient = new ShareClient(storageConnectionString, shareName);     // All letters in a share name must be lowercase.
            _shareClient.CreateIfNotExists();

            _shareDirectory = _shareClient.GetDirectoryClient(directoryName);
            _shareDirectory.CreateIfNotExists();
        }

        /// <summary>
        /// Delete the Azure file share.
        /// </summary>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> DeleteAsync()
        {
            bool success = false;

            try
            {
                success = await _shareClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                OnStatusUpdate($"An error occurred deleting file share {_shareName}. {ex}", OperationStatus.Error);
            }

            // Final status notification
            if (success == true)
            {
                OnStatusUpdate($"{_shareName} file share was successfully deleted.", OperationStatus.Success);
            }
            else
            {
                OnStatusUpdate($"{_shareName} file share was not successfully deleted.", OperationStatus.Error);
            }

            return success;
        }

        /// <summary>
        /// Create the Azure file share directory.
        /// </summary>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> CreateAsync()
        {
            bool success = false;

            const int MaxTries = 60;
            int count = 0;
            do
            {
                try
                {
                    await _shareClient.CreateIfNotExistsAsync();
                    count = MaxTries + 1;
                    success = true;
                }
                catch (RequestFailedException)
                {
                    OnStatusUpdate($"{count + 1}: The {_shareName} file share is currently being created. Please wait.", OperationStatus.Warning);
                    await Task.Delay(1000); // The share is currently being deleted. Try again until it works.
                }
                catch (Exception ex)
                {
                    // Catch-all
                    OnStatusUpdate($"An error occurred creating file share {_shareName}. {ex}", OperationStatus.Error);
                }

                count++;

            } while (count < MaxTries);

            // Final status notification
            if (success == true)
            {
                OnStatusUpdate($"{_shareName} file share was successfully created.", OperationStatus.Success);
            }
            else
            {
                OnStatusUpdate($"{_shareName} file share was not successfully created. Retry attempts: {count}.", OperationStatus.Error);
            }

            return success;
        }

        /// <summary>
        /// Delete the Azure file share directory.
        /// </summary>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> DeleteDirectoryAsync()
        {
            bool success = false;

            try
            {
                success = await _shareDirectory.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                OnStatusUpdate($"An error occurred deleting directory {_directoryName}. {ex}", OperationStatus.Error);
            }

            // Final status notification
            if (success == true)
            {
                OnStatusUpdate($"{_directoryName} directory was successfully deleted.", OperationStatus.Success);
            }
            else
            {
                OnStatusUpdate($"{_directoryName} directory was not successfully deleted.", OperationStatus.Error);
            }

            return success;
        }

        /// <summary>
        /// Create the Azure file share directory.
        /// </summary>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> CreateDirectoryAsync()
        {
            bool success = false;

            const int MaxTries = 60;
            int count = 0;
            do
            {
                try
                {
                    await _shareDirectory.CreateIfNotExistsAsync();
                    count = MaxTries + 1;
                    success = true;
                }
                catch (RequestFailedException)
                {
                    OnStatusUpdate($"{count + 1}: The {_directoryName} directory is currently being creating. Please wait.", OperationStatus.Warning);
                    await Task.Delay(1000); // The directory is currently being deleted. Try again until it works.
                }
                catch (Exception ex)
                {
                    // Catch-all
                    OnStatusUpdate($"An error occurred creating directory {_directoryName}. {ex}", OperationStatus.Error);
                }

                count++;

            } while (count < MaxTries);

            // Final status notification
            if (success == true)
            {
                OnStatusUpdate($"{_directoryName} directory was successfully created.", OperationStatus.Success);
            }
            else
            {
                OnStatusUpdate($"{_directoryName} directory was not successfully created. Retry attempts: {count}.", OperationStatus.Error);
            }

            return success;
        }

        /// <summary>
        /// Add or update the file.
        /// </summary>
        /// <param name="file">File.</param>
        /// <returns><see cref="bool"/> True if success. False if otherwise.</returns>
        public async Task<bool> AddOrUpdateAsync(TFile file)
        {
            if (file is null)
            {
                OnStatusUpdate($"nameof{file} was null. Unable to persist to file share {_shareName}/{_directoryName}.", OperationStatus.Error);
                return false;
            }

            //  Azure allows for 4MB max uploads  (4 x 1024 x 1024 = 4194304)
            const int UploadLimit = 4194304;

            try
            {
                using (var stream = new MemoryStream(file.Contents))
                {
                    stream.Seek(0, SeekOrigin.Begin);   // ensure stream is at the beginning
                    var fileClient = await _shareDirectory.CreateFileAsync(file.Name, stream.Length);

                    // If stream is below the limit upload directly
                    if (stream.Length <= UploadLimit)
                    {
                        var result = await fileClient.Value.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
                        OnStatusUpdate($"Persisted '{file.Name}' to file share {_shareName}/{_directoryName}.", OperationStatus.Success);
                        return true;
                    }

                    int bytesRead;
                    long index = 0;
                    byte[] buffer = new byte[UploadLimit];

                    // Stream is larger than the limit so we need to upload in chunks
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        // Create a memory stream for the buffer to upload
                        using MemoryStream ms = new(buffer, 0, bytesRead);
                        var result = await fileClient.Value.UploadRangeAsync(new HttpRange(index, ms.Length), ms);
                        index += ms.Length; // increment the index to the account for bytes already written
                    }
                }

                OnStatusUpdate($"Persisted '{file.Name}' to file share {_shareName}/{_directoryName}.", OperationStatus.Success);
                return true;
            }
            catch (Exception ex)
            {
                OnStatusUpdate($"An error occurred persisting '{file.Name}' to file share {_shareName}/{_directoryName}. {ex}", OperationStatus.Error);
                return false;
            }
        }

        /// <summary>
        /// Based on the filename in the existing directory, get the file contents as a byte array.
        /// </summary>
        /// <param name="fileName">Name of existing file.</param>
        /// <returns><see cref="File"/> or null if file not found.</returns>
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

                        using var memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                        file.Contents = memoryStream.ToArray();
                    }

                    OnStatusUpdate($"Successfully obtained {fileName} from file share {_shareName}/{_directoryName}.", OperationStatus.Success);
                }
                else
                {
                    OnStatusUpdate($"{fileName} not found in file share {_shareName}/{_directoryName}.", OperationStatus.Error);
                }
            }
            catch (Exception ex)
            {
                OnStatusUpdate($"An error occurred obtaining {fileName} from file share {_shareName}/{_directoryName}. {ex}", OperationStatus.Error);
            }

            return file;
        }

        /// <summary>
        /// Event handler to notify caller of status.
        /// </summary>
        /// <param name="message">Status message.</param>
        /// <param name="operationStatus">Status of the operation.</param>
        private void OnStatusUpdate(string message, OperationStatus operationStatus)
        {
            this.StatusUpdate?.Invoke(this, new FileShareEventArgs(message, operationStatus));
        }
    }
}
