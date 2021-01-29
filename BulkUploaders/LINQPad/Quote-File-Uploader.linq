<Query Kind="Program">
  <NuGetReference>Azure.Storage.Files.Shares</NuGetReference>
  <NuGetReference>Microsoft.Azure.Cosmos.Table</NuGetReference>
  <NuGetReference>System.Linq.Async</NuGetReference>
  <Namespace>Microsoft.Azure.Cosmos.Table</Namespace>
  <Namespace>Microsoft.Azure.Cosmos.Table.Protocol</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Drawing2D</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Azure.Storage.Files.Shares</Namespace>
  <Namespace>Azure</Namespace>
  <Namespace>Azure.Storage.Files.Shares.Models</Namespace>
</Query>

async Task Main()
{
	// This is a data and file uploader for Noftware In.Faux's quotes.
	// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	// This script performs the following tasks:
	// 1. Reads a plain-text file that contains one or more quotes.
	// 2. Stores the contents of the text file in Azure table storage.
	// 3. If necessary, each image is resized to a defined maximum dimension.
	// 4. Images (both original and resized) are persisted to an Azure file share.

	// ~~~~~~
	// Q: WHAT SHOULD I DO NOW?
	// A: Edit the settings below to match your environment and data and run this script. Search for ~modify~ to find each of the settings.
	// ~~~~~~

	Console.WriteLine("[ Quote data and file Azure persistence for Noftware In.Faux ]");
	Console.WriteLine();

	// ============
	// Line format
	// ============
	// ex: Circle back {Following up on progress.} [CircleBack.png] |reach,contact,follow,up,follow-up,communicate|
	// * First section contains the quote.
	// * Second section (within {}) contains the description.
	// * Third section (within []) contains the file name. Do not include a folder/directory. That is defined in the LocalImageDirectory, below.
	// * The forth and final section (within ||) contain zero or more associated keywords. They are used in the app's search functionality.

	// (~modify~) Local folder/directory where the quote's associated images are located
	const string LocalImageDirectory = @"C:\MyQuotes\Images";
	// (~modify~) The text file that contains the quote data. One line per quote.
	const string LocalSourceFile = @"C:\MyQuotes\quotes.txt";
	// (~modify~) The maximum width or height of the resized image. Both the original and resized images are upload to Azure file share.
	const double MaxImageDimension = 600;

	// (~modify~) Name of the table in Azure table storage
	const string AzureTableStorageName = "Quote";
	// (~modify~) Name of table partition in Azure table storage
	const string AzureTablePartitionKey = "Quotes";
	// (~modify~) The name of the file share where both /original and /resized image folders are created. Must be lower case.
	const string AzureFileShareName = "quote";

	// (~modify~) Azure storage connection string
	const string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=[[redacted]];AccountKey=[[redacted]];EndpointSuffix=core.windows.net";

	// Output the settings
	Console.WriteLine("~~~~~~~~~~");
	Console.WriteLine($"My local quote image directory: {LocalImageDirectory}");
	Console.WriteLine($"My local quote source text file: {LocalSourceFile}");
	Console.WriteLine($"Maximum image height or width: {MaxImageDimension}");
	Console.WriteLine($"Azure table storage name: {AzureTableStorageName}");
	Console.WriteLine($"Azure table partition key: {AzureTablePartitionKey}");
	Console.WriteLine($"Azure file share folder name: {AzureFileShareName}");
	Console.WriteLine("~~~~~~~~~~");
	Console.WriteLine();

	var settings = new QuoteParserSettings()
	{
		QuoteTextFile = LocalSourceFile,
		InputImagePath = LocalImageDirectory,
		MaximumImageDimension = MaxImageDimension
	};

	// Table storage for the quote data
	ITableRepository<QuoteTableEntity> tableRepo = new TableRepository<QuoteTableEntity>(AzureStorageConnectionString, AzureTableStorageName);
	await tableRepo.DeleteTableAsync();
	await tableRepo.CreateTableAsync();

	// File share storage for the original images
	IFileShareRepository fileRepoOriginal = new FileShareRepository(AzureStorageConnectionString, AzureFileShareName, "original");
	await fileRepoOriginal.DeleteAsync();
	await fileRepoOriginal.CreateAsync();
	await fileRepoOriginal.CreateDirectoryAsync();

	// File share storage for the resized images
	IFileShareRepository fileRepoResized = new FileShareRepository(AzureStorageConnectionString, AzureFileShareName, "resized");
	await fileRepoResized.CreateDirectoryAsync();

	// Parse the flat file into ParsedQuote items
	var quoteParser = new QuoteParser(settings);
	var parsedQuotes = quoteParser.ParseInputFileAsync();
	int i = 0;
	await foreach (var parsedQuote in parsedQuotes)
	{
		i++;
		string output = $"{i}: {parsedQuote}";
		Console.WriteLine(output);
		if (parsedQuote.Errors?.Any() == true)
		{
			parsedQuote.Errors.Dump();
		}

		// Map to Azure Table entity and persist
		var quoteTableEntity = new QuoteTableEntity()
		{
			FileName = parsedQuote.FileName,
			OriginalImageHeight = parsedQuote.OriginalImageHeight,
			OriginalImageWidth = parsedQuote.OriginalImageWidth,
			ResizedImageHeight = parsedQuote.ResizedImageHeight,
			ResizedImageWidth = parsedQuote.ResizedImageWidth,
			KeyWords = parsedQuote.DelimitedKeyWords,
			Description = parsedQuote.Description,
			Text = parsedQuote.Text,
			Timestamp = DateTimeOffset.UtcNow,
			PartitionKey = AzureTablePartitionKey,
			RowKey = System.Guid.NewGuid().ToString()
		};
		var result = await tableRepo.AddOrUpdateAsync(quoteTableEntity);

		// Persist original and resized images to Azure file share
		await fileRepoOriginal.AddOrUpdateAsync(parsedQuote.OriginalImage, parsedQuote.FileName);
		await fileRepoResized.AddOrUpdateAsync(parsedQuote.ResizedImage, parsedQuote.FileName);
	}
}

/// <summary>
/// Parses a text file that contains quote data.
/// Format: Quote text {Meaning/explanation text} [FileName] |Comma-separated keywords/tags|
/// Example: Circle back {Following up on progress.} [CircleBack.png] |reach,contact,follow,up,follow-up,communicate|
/// </summary>
public class QuoteParser : IQuoteParser
{
	// Settings for the parser
	private readonly QuoteParserSettings _settings;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="settings">Settings for the parser.</param>
	public QuoteParser(QuoteParserSettings settings)
	{
		_settings = settings;
	}

	/// <summary>
	/// Parses the quote text file.
	/// </summary>
	/// <returns><see cref="IAsyncEnumerable{ParsedQuote}"/></returns>
	public async IAsyncEnumerable<ParsedQuote> ParseInputFileAsync()
	{
		if (System.IO.File.Exists(_settings.QuoteTextFile) == false)
		{
			yield return null;
		}

		string cleanLine;
		int index1;
		int index2;
		string filename;
		string text;
		string description;
		string keywords;

		string[] lines = await System.IO.File.ReadAllLinesAsync(_settings.QuoteTextFile);
		foreach (var line in lines)
		{
			cleanLine = line?.Trim();
			if (string.IsNullOrEmpty(cleanLine) == false)
			{
				// Any parsed errors
				var parserErrors = new List<string>();

				// One parsed quote per line in the text file
				var parsedQuote = new ParsedQuote()
				{
					Line = cleanLine
				};

				// ex: Circle back {Following up on progress.} [CircleBack.png] |reach,contact,follow,up,follow-up,communicate|
				if (cleanLine.Contains("{") == true && cleanLine.Contains("}") == true &&
				cleanLine.Contains("[") == true && cleanLine.Contains("]") == true)
				{
					// Meaning: Ensure index2 is after index1
					index1 = cleanLine.IndexOf("{");
					index2 = cleanLine.IndexOf("}");
					if (index2 > index1)
					{
						description = cleanLine.Substring(index1 + 1, index2 - index1 - 1)?.Trim();
						parsedQuote.Description = description;
					}
					else
					{
						parserErrors.Add("The description's closing '}' appears before the text's opening '{'.");
					}

					// Quote: Get from the start to the first opening '{'
					text = cleanLine.Substring(0, index1 - 1);
					parsedQuote.Text = text;

					// Image: Ensure index2 is after index1
					index1 = cleanLine.IndexOf("[");
					index2 = cleanLine.IndexOf("]");
					if (index2 > index1)
					{
						filename = cleanLine.Substring(index1 + 1, index2 - index1 - 1)?.Trim();
						parsedQuote.FileName = filename;

						LoadImage(parsedQuote);
						if (parsedQuote.OriginalImage != null)
						{
							ResizeImage(parsedQuote);
						}
					}
					else
					{
						parserErrors.Add("The file name's closing ']' appears before the file name's opening '['.");
					}

					// Keywords: Ensure index2 is after index1
					index1 = cleanLine.IndexOf("|");
					index2 = cleanLine.LastIndexOf("|");
					if (index2 > index1)
					{
						keywords = cleanLine.Substring(index1 + 1, index2 - index1 - 1)?.Trim();
						if (string.IsNullOrWhiteSpace(keywords) == false)
						{
							keywords = keywords.Replace(" ", string.Empty);
							parsedQuote.KeyWords = keywords.Split(new char[] { ',' });
						}
						else
						{
							parserErrors.Add("The keywords are not defined between the '|' delimiters..");
						}
					}
					else
					{
						parserErrors.Add("The keywords are not defined.");
					}

					yield return parsedQuote;
				}
			} // if (string.IsNullOrEmpty(cleanLin...
		} // foreach (var line in lines)
	}

	/// <summary>
	/// Based on a filename in the parsedQuote, load the image from disk into the OriginalImage byte array.
	/// </summary>
	/// <param name="parsedQuote">Parsed quote with filename.</param>
	private void LoadImage(ParsedQuote parsedQuote)
	{
		string filename = parsedQuote.FileName;
		string imageFile = System.IO.Path.Combine(_settings.InputImagePath, filename);

		if (System.IO.File.Exists(imageFile) == false)
		{
			// File not found
			var errors = new List<string>();
			if (parsedQuote.Errors?.Any() == true)
			{
				errors.AddRange(parsedQuote.Errors);
			}
			errors.Add($"Image '{imageFile}' not found.");
			parsedQuote.Errors = errors;

			parsedQuote.OriginalImage = null;
			parsedQuote.OriginalImageHeight = 0;
			parsedQuote.OriginalImageWidth = 0;
		}
		else
		{
			// Load file and store as byte array
			var image = Image.FromFile(imageFile);
			parsedQuote.OriginalImageHeight = image.Height;
			parsedQuote.OriginalImageWidth = image.Width;

			using (var ms = new MemoryStream())
			{
				image.Save(ms, image.RawFormat);
				parsedQuote.OriginalImage = ms.ToArray();
			}
		}
	}

	/// <summary>
	/// Resize the OriginalImage byte array to a maximum width or height and store in the ResizedImage byte array.
	/// </summary>
	/// <param name="parsedQuote">Parsed quote with populated OriginalImage.</param>
	private void ResizeImage(ParsedQuote parsedQuote)
	{
		int origWidth = parsedQuote.OriginalImageWidth;
		int origHeight = parsedQuote.OriginalImageHeight;

		// Ratio between height and width
		double resizeRatio;

		// Is this a square or rectangular image?
		bool squareImage = (origWidth == origHeight);

		// If width is greater than the maximum, resize the width.
		// If height is greater than the maximum, resize the height.
		double newWidth = origWidth;
		double newHeight = origHeight;
		if (squareImage == true)
		{
			// Square image
			if (origWidth > _settings.MaximumImageDimension)
			{
				newWidth = _settings.MaximumImageDimension;
				newHeight = _settings.MaximumImageDimension;
			}
		}
		else
		{
			// Rectangular image
			if (origWidth > origHeight)
			{
				// Landscape: Resize the width
				if (origWidth > _settings.MaximumImageDimension)
				{
					resizeRatio = (double)_settings.MaximumImageDimension / (double)origWidth;
					newWidth = _settings.MaximumImageDimension;
					newHeight = origHeight * resizeRatio;
				}
			}
			else
			{
				// Portrait: Resize the height
				if (origHeight > _settings.MaximumImageDimension)
				{
					resizeRatio = (double)_settings.MaximumImageDimension / (double)origHeight;
					newHeight = _settings.MaximumImageDimension;
					newWidth = origWidth * resizeRatio;
				}
			}
		}

		// Convert byte array to Bitmap
		Image srcImage = (Bitmap)((new ImageConverter()).ConvertFrom(parsedQuote.OriginalImage));

		parsedQuote.ResizedImageHeight = (int)newHeight;
		parsedQuote.ResizedImageWidth = (int)newWidth;

		var destImage = new Bitmap((int)newWidth, (int)newHeight);
		destImage.SetResolution(72, 72);    // 72 DPI

		using (var graphics = Graphics.FromImage(destImage))
		{
			// Leave as defaults for slightly smaller images
			// =====
			//graphics.CompositingMode = CompositingMode.SourceCopy;
			//graphics.CompositingQuality = CompositingQuality.HighQuality;
			//graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			//graphics.SmoothingMode = SmoothingMode.HighQuality;
			//graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			graphics.DrawImage(srcImage, 0, 0, parsedQuote.ResizedImageWidth, parsedQuote.ResizedImageHeight);
			using (var ms = new MemoryStream())
			{
				destImage.Save(ms, srcImage.RawFormat);
				parsedQuote.ResizedImage = ms.ToArray();
			}
		} // using (var graphics = Graphics.FromImage(destImage))

		destImage.Dispose();
		srcImage.Dispose();
	}
}

/// <summary>
/// Parses a text file that contains quote data.
/// Format: Quote text {Meaning/explanation text} [FileName] |Comma-separated keywords/tags|
/// Example: Circle back {Following up on progress.} [CircleBack.png] |reach,contact,follow,up,follow-up,communicate|
/// </summary>
public interface IQuoteParser
{
	/// <summary>
	/// Parses the quote text file.
	/// </summary>
	/// <returns><see cref="IAsyncEnumerable{ParsedQuote}"/></returns>
	IAsyncEnumerable<ParsedQuote> ParseInputFileAsync();
}

/// <summary>
/// Settings for the quote flat file parser and image resizing.
/// </summary>
public class QuoteParserSettings
{
	/// <summary>
	/// Maximum dimension in pixels.
	/// </summary>
	public double MaximumImageDimension { get; set; }

	/// <summary>
	/// Path to where all images are located.
	/// </summary>
	public string InputImagePath { get; set; }

	/// <summary>
	/// Path and name of quote source file.
	/// </summary>
	public string QuoteTextFile { get; set; }
}

/// <summary>
/// A quote parsed from a source text file. 
/// </summary>
public class ParsedQuote
{
	/// <summary>
	/// Source line.
	/// </summary>
	public string Line { get; set; }

	/// <summary>
	/// A list of parser errors. If empty, parse was a success.
	/// </summary>
	public IEnumerable<string> Errors { get; set; } = new List<string>();

	/// <summary>
	/// Name of the image file.
	/// </summary>
	public string FileName { get; set; }

	/// <summary>
	/// Associated keywords.
	/// </summary>
	public IEnumerable<string> KeyWords { get; set; } = new List<string>();

	/// <summary>
	/// The text.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// The description.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Original image.
	/// </summary>
	public byte[] OriginalImage { get; set; }

	/// <summary>
	/// Original image's width (in pixels).
	/// </summary>
	public int OriginalImageWidth { get; set; }

	/// <summary>
	/// Original image's height (in pixels).
	/// </summary>
	public int OriginalImageHeight { get; set; }

	/// <summary>
	/// Resized image.
	/// </summary>
	public byte[] ResizedImage { get; set; }

	/// <summary>
	/// Resized image's width (in pixels).
	/// </summary>
	public int ResizedImageWidth { get; set; }

	/// <summary>
	/// Resized image's height (in pixels).
	/// </summary>
	public int ResizedImageHeight { get; set; }

	/// <summary>
	/// Associated keywords, delimited by a comma.
	/// </summary>
	public string DelimitedKeyWords
	{
		get
		{
			string output = string.Empty;

			if (this.KeyWords?.Any() == true)
			{
				output = string.Join(',', this.KeyWords);
			}

			return output;
		}
	}

	/// <summary>
	/// Formatted output suitable for display.
	/// </summary>
	public override string ToString()
	{
		if (this.KeyWords?.Any() == true)
		{
			return $"{this.Text} | {this.Description} | {this.FileName} | Keywords: {this.DelimitedKeyWords}";
		}
		else
		{
			return $"{this.Text} | {this.Description} | {this.FileName} | *No Keywords*";
		}
	}
}

/// <summary>
/// Azure Table entity for quote.
/// </summary>
public class QuoteTableEntity : TableEntity
{
	/// <summary>
	/// The text.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// The description.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Name of the original image file.
	/// </summary>
	public string FileName { get; set; }

	/// <summary>
	/// Original image's width (in pixels).
	/// </summary>
	public int OriginalImageWidth { get; set; }

	/// <summary>
	/// Original image's height (in pixels).
	/// </summary>
	public int OriginalImageHeight { get; set; }

	/// <summary>
	/// Resized image's width (in pixels).
	/// </summary>
	public int ResizedImageWidth { get; set; }

	/// <summary>
	/// Resized image's height (in pixels).
	/// </summary>
	public int ResizedImageHeight { get; set; }

	/// <summary>
	/// Associated keywords, separated by commas.
	/// </summary>
	public string KeyWords { get; set; }

	// The PartitionKey property stores string values that identify the partition that an entity belongs to. Entities that have the same PartitionKey value are stored in the same partition.</param>
	// The RowKey property stores string values that uniquely identify entities within each partition.</param>
}

/// <summary>
/// Azure file share repository.
/// </summary>
public interface IFileShareRepository
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
	/// <param name="fileContents">File content byte array.</param>
	/// <param name="fileName">Name of the file.</param>
	/// <returns><see cref="ShareFileUploadInfo"/></returns>
	Task<ShareFileUploadInfo> AddOrUpdateAsync(byte[] fileContents, string fileName);

	/// <summary>
	/// Based on the filename in the existing directory, get the file contents as a byte array.
	/// </summary>
	/// <param name="fileName">Name of existing file.</param>
	/// <returns>byte[] or null if file not found.</returns>
	Task<byte[]> GetFileAsync(string fileName);
}

/// <summary>
/// Azure file share repository.
/// </summary>
public class FileShareRepository : IFileShareRepository
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
	/// <param name="tableName">Azure file share name.</param>
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
	/// <param name="fileContents">File content byte array.</param>
	/// <param name="fileName">Name of the file.</param>
	/// <returns><see cref="ShareFileUploadInfo"/></returns>
	public async Task<ShareFileUploadInfo> AddOrUpdateAsync(byte[] fileContents, string fileName)
	{
		//  Azure allows for 4MB max uploads  (4 x 1024 x 1024 = 4194304)
		const int UploadLimit = 4194304;

		ShareFileUploadInfo result = null;

		var stream = new MemoryStream(fileContents);

		stream.Seek(0, SeekOrigin.Begin);   // ensure stream is at the beginning
		var fileClient = await _shareDirectory.CreateFileAsync(fileName, stream.Length);

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
	/// <returns>byte[] or null if file not found.</returns>
	public async Task<byte[]> GetFileAsync(string fileName)
	{
		byte[] fileData = null;

		var fileClient = _shareDirectory.GetFileClient(fileName);
		if (await fileClient.ExistsAsync())
		{
			using (var stream = await fileClient.OpenReadAsync(new ShareFileOpenReadOptions(false)))
			{
				using (var memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					fileData = memoryStream.ToArray();
				}
			}
		}

		return fileData;
	}
}

/// <summary>
/// Azure table repository.
/// </summary>
/// <typeparam name="TEntity">Model of type TableEntity.</typeparam>
public class TableRepository<TEntity> : ITableRepository<TEntity> where TEntity : TableEntity, new()
{
	// Connection
	private readonly CloudTableClient _tableClient;

	// Table to be persisted to
	private readonly CloudTable _table;

	// Azure table name
	private readonly string _tableName;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="storageConnectionString">Azure storage connection string.</param>
	/// <param name="tableName">Azure table name.</param>
	public TableRepository(string storageConnectionString, string tableName)
	{
		_tableName = tableName;

		CloudStorageAccount account = CloudStorageAccount.Parse(storageConnectionString);
		_tableClient = account.CreateCloudTableClient();

		// Create 'table' if it does not exist
		_table = _tableClient.GetTableReference(tableName);
		_table.CreateIfNotExists();
	}

	/// <summary>
	/// Delete the Azure table.
	/// </summary>
	public async Task DeleteTableAsync()
	{
		await _table.DeleteAsync();
	}

	/// <summary>
	/// Create the Azure table.
	/// </summary>
	public async Task CreateTableAsync()
	{
		// https://stackoverflow.com/questions/15508517/the-correct-way-to-delete-and-recreate-a-windows-azure-storage-table-error-409
		const int MaxTries = 60;
		int count = 0;
		do
		{
			try
			{
				await _table.CreateIfNotExistsAsync();
				count = MaxTries + 1;
			}
			catch (StorageException e)
			{
				if ((e.RequestInformation.HttpStatusCode == 409) && (e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted)))
				{
					Console.WriteLine($"{count + 1}: The '{_tableName}' table is currently being deleted. Please wait.");
					await Task.Delay(1000); // The table is currently being deleted. Try again until it works.
				}
				else
				{
					throw;
				}
			}

			count++;

		} while (count < MaxTries);
	}

	/// <summary>
	/// Add or update the Azure table.
	/// </summary>
	/// <param name="tableEntity">Azure table.</param>
	/// <returns><see cref="TableResult"/></returns>
	public async Task<TableResult> AddOrUpdateAsync(TEntity tableEntity)
	{
		var operation = TableOperation.InsertOrReplace(tableEntity);
		var result = await _table.ExecuteAsync(operation);

		return result;
	}

	/// <summary>
	/// Get a random item based on the specified partition key and either the first item greater than rowKey, less than rowKey, or null if not found.
	/// </summary>
	/// <returns><see cref="TEntity"/></returns>
	public async Task<TEntity> GetRandomAsync(string partitionKey, string rowKey)
	{
		TEntity entity;

		// Try greater than (>)
		entity = await this.GetRandomByComparisonOperatorAsync("gt", partitionKey, rowKey);
		if (entity != null)
		{
			// Return greater than
			return entity;
		}
		else
		{
			// Try less than (<). It will either contain an item or be null. Either way, return it.
			entity = await this.GetRandomByComparisonOperatorAsync("lt", partitionKey, rowKey);
			return entity;
		}
	}

	/// <summary>
	/// Get a random item based on the specified partition key and either the first item greater than rowKey, less than rowKey, or null if not found.
	/// </summary>
	/// <param name="comparisonOperator">OData comparison operators, i.e. gt, lt, eq, etc.</param>
	/// <param name="partitionKey">Partition key.</param>
	/// <param name="rowKey">Row key used in the greater than or less than comparison.</param>
	/// <returns><see cref="TEntity"/></returns>
	private async Task<TEntity> GetRandomByComparisonOperatorAsync(string comparisonOperator, string partitionKey, string rowKey)
	{
		TableContinuationToken token = null;
		var q = new TableQuery<TEntity>()
		{
			FilterString = $"PartitionKey eq '{partitionKey}' and RowKey {comparisonOperator} '{rowKey}'",
			TakeCount = 1
		};

		var queryResult = await _table.ExecuteQuerySegmentedAsync(q, token);
		token = queryResult.ContinuationToken;
		if (queryResult.Results.Any() == true)
		{
			return queryResult.Results.First();
		}

		return null;
	}

	/// <summary>
	/// Get all table entity items from the table.
	/// </summary>
	/// <returns><see cref="IAsyncEnumerable{TEntity}"/></returns>
	public async IAsyncEnumerable<TEntity> GetAllAsync()
	{
		TableContinuationToken token = null;
		do
		{
			var q = new TableQuery<TEntity>();
			var queryResult = await _table.ExecuteQuerySegmentedAsync(q, token);
			foreach (var item in queryResult.Results)
			{
				yield return item;
			}
			token = queryResult.ContinuationToken;

		} while (token != null);
	}
}

/// <summary>
/// Azure table repository interface.
/// </summary>
/// <typeparam name="TEntity">Model of type TableEntity.</typeparam>
public interface ITableRepository<TEntity> where TEntity : TableEntity, new()
{
	/// <summary>
	/// Delete the Azure table.
	/// </summary>
	Task DeleteTableAsync();

	/// <summary>
	/// Create the Azure table.
	/// </summary>
	Task CreateTableAsync();

	/// <summary>
	/// Add or update the Azure table.
	/// </summary>
	/// <param name="tableEntity">Azure table.</param>
	/// <returns><see cref="TableResult"/></returns>
	Task<TableResult> AddOrUpdateAsync(TEntity tableEntity);

	/// <summary>
	/// Get a random item based on the specified partition key and either the first item greater than rowKey, less than rowKey, or null if not found.
	/// </summary>
	/// <param name="partitionKey">Partition key.</param>
	/// <param name="rowKey">Row key used in the greater than or less than comparison.</param>
	/// <returns><see cref="TEntity"/></returns>
	Task<TEntity> GetRandomAsync(string partitionKey, string rowKey);

	/// <summary>
	/// Get all table entity items from the table.
	/// </summary>
	/// <returns><see cref="IAsyncEnumerable{TEntity}"/></returns>
	IAsyncEnumerable<TEntity> GetAllAsync();
}