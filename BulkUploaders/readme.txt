There are multiple ways to bulk upload your quotes. Chose the one that works best for you.

1. LINQPad - LINQPad (https://www.linqpad.net/) is a great scripting (C#, VB.NET, F#, etc.) tool. Bulk upload scripts were written in c# to add the search stop words and quotes to Azure table storage. Likewise, the images were resized and uploaded to Azure file shares in the same manner. LINQPad comes with free and paid versions.
   a. (optional, but recommended) Quote-Stop-Words.linq - this will persist the list of stop words to Azure table so when the quote's content is indexed, it won't contain these stop words (aka noise words), i.e. 'the', 'but', 'what', etc.
   b. Quote-File-Uploader.linq - this is the quote data and image uploader. If the quote words are populated from the above step, the search index content will not contain these stop words/noise words.
   
*** Coming soon ***
2. Console application (.NET Core) to upload the JSON quotes and search stop words to Azure table storage. Likewise, the images were resized and uploaded to Azure file shares in the same manner.
