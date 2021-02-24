# in.faux random quote generator
in.faux random quote generator can be used to display any type of information with an associated image.

**Examples**:
- Word of the day.
- Cat photo of the day.
- Phrase of the day.
- English to Spanish translation.
- Inspirational quote.
- Showcase amazing photos.
- _or anything else you can think of!_

**Showcase sites**:
- [Business Cliché Generator](https://www.businessclichegenerator.com/r "Business Cliché Generator")

**Solution technologies**:
- [Azure Static Web App](https://docs.microsoft.com/en-us/azure/static-web-apps/deploy-blazor "Azure Static Web App")
- [Blazor WebAssembly (WASM)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor "Blazor WebAssembly (WASM)")
- [Azure Function](https://docs.microsoft.com/en-us/azure/azure-functions/functions-overview "Azure Function")
- [Azure Table Storage](https://azure.microsoft.com/en-us/services/storage/tables/ "Azure Table Storage")
- [Azure File Shares](https://docs.microsoft.com/en-us/azure/storage/files/storage-files-introduction "Azure File Shares")

**Assumptions**:
- You have an existing GitHub account.
- You have an existing Azure subscription (free or paid).

**Solution projects**
- **In.Faux.Client** - WASM client (.NET 5)
- **In.Faux.Core** - Library/shared project (.NET Standard 2.1)
- **In.Faux.Data** - Data project (.NET Standard 2.1)
- **In.Faux.Function** - Azure HttpTrigger Function (.NET Core 3.1)
- **In.Faux.BulkUploader** - Console application to bulk upload data to Azure table storage (.NET Core 3.1)

### Setup and configuration - static web app:
0. In the Azure web portal, create a new **Resource Group** (or use an existing one).
1. In the resource group, create **Table storage** (or use an existing one, if you already have one provisioned). Note: The tables will automatically be created. They are called *Quote*, *QuoteImpression*, *QuoteMetadata*, and *QuoteSearchIndex*.

**If you are debugging/testing in Visual Studio and are not yet ready to deploy to Azure, ignore steps 3-13 below. Instead follow these two steps:**
- In the **Client** project, uncomment *ApiBaseAddress* so that \"http://localhost:7071" is enabled.
- In the **Function** project, set the *TableStorageConnectionString* setting with the table storage connection string. You can obtain your table storage connection string via the *Access keys* menu item in your storage account (created in step 1).

**If you are ready to deploy to Azure, follow steps 3-13:**

3. In the Client project, comment out ApiBaseAddress so that it is not available.
4. Go back to the resource group, click **Add**, and search for *static web app*.
5. In the **Static Web App** creation page, enter the *Name* and choose the *Region* nearest you.
6. Click the **Sign in with GitHub** button. If you are not already signed into GitHub, enter you GitHub credentials.
7. An **Authorize Azure Static Web Apps** page appears. Click the *Authorize* button.
8. The name of your GitHub appears along with dropdown fields for **Organization**, **Repository**, and **Branch**.
9. Choose your *Organization*, *Repository*, and *Branch*.
10. Choose *Blazor* for the **Build Presets**.
11. For **App location** specify *Client*, for **API location** specify *Function*, and for **Output location** specify *wwwroot*.
12. Select **Review + create** button, review and follow any remaining steps.
13. Once the app is deployed (check github\'s Actions for completion), go to yo ur newly created **Static Web App** in the Azure Portal, click the **Configuration** menu item, and add a new setting called *TableStorageConnectionString*. You can obtain your table storage connection string via the *Access keys* menu item in your storage account (created in step 1).

### Setup and configuration - input file:
At time of writing, the only means to upload data is via the provided console application and its proprietary text file. The reason for the proprietary text file was it was easy to keep it updated via notepad (as opposed to a structured format like JSON or XML). The next version will include web forms to administer quotes.

**Input text file format**:
All elements of a quote must be on a single line.
*Example*:
> Himalayan cat {The Himalayan, is a breed or sub-breed of long-haired cat like the Persian, except for its blue eyes.} [Himalayan1.png] |persian,himmy,siamese|

**Structure and data elements**:

0. Quote text.
1. Quote explanation (Between **\{** and **\}** tags. Optional but empty **\{\}** tags are required.).
2. File name (Between **\[** and **\]** tags. Optional but empty **\[\]** tags are required. *Do not enter a path/directory. That will be specified via the console app\'s settings.*)
3. Additional index keywords (Between **\|** and **\|** tags. Separate multiple keywords via commas. Optional but empty **\|\|** tags are required.)

### Setup and configuration - data uploader:
0. Build the application.
1. Edit **appsettings.json**:
- In the *StorageConnectionString* setting, set the same table storage connection string, as above.
- Adjust *MaximumResizedImageDimension* and *MaximumThumbnailImageDimension* to suit your needs.
- Set the full path to your images in the *InputImagePath* settings.
- In the *QuoteTextFile* settings, specify the full file path and file name to your quote text file.
2. Run **BulkUploader.exe** and when prompted, specify either *append* to append data to the existing table storage (i.e. if you are adding new quotes) or *overwrite* for net-new data.
3. You should now be able to run the client WASM app in your browser to see your quote data!
