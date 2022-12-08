# cognitive-function
Azure Functions integration with Cognitive Services

This function scans for a blob-storage container (specified by env variable ```Container```) for images and send them over to Cognitive Services for Read-In.

The result is filtered by RegExp provided in env variable ```RegExpMatcher```

The filtered result is loaded in CosmosDB database provided in env variable ```Database``` and Collection ```Collection```

The cognitive services instance is configured to env variables ```CsUrl``` and  ```CsAccessKey```


There is PDF splitting Function in action as well configured with the input container ```PdfSplitter``` and output container ``PdfSplitteOutput```. These can be same as well.

The plitter can share container with form-recognizer. A filtering variable was added to support that: ```FRFilter```. Even if the container are not shared, the filter should be at least '.pdf'.

Container names can include directory names


## Running locally

Just create a local.settings.json with following:


```
{
    "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "Storage_Landing": "CONNECTION STRING TO MY STORAGE ACCOUNT",
    "CsUrl": "https:/MY-COGNITIVE-SERVICES-INSTANCE.cognitiveservices.azure.com/",
    "CsAccessKey": "MY-COGNITIVE-SERVICES-INSTANCE-ACCESSS-KEY",
    "RegExpMatcher": "SOMETHING|^y$",
    "Container": "blob-container-name-for-OCR",
    "ContainerFR": "blob-container-name-for-form-recognizer",
    "FRFilter": "File-ending filter for form-recognizer function (for example .pdf or page1.pdf)",
    "PdfSplitter": "blob-container-name-for-input-of-splitting-pdf",
    "PdfSplitteOutput": "blob-container-name-for-output-of-splitting-pdf (can be same as input)",
    "ModelId": "ID of the trained model of form-recognizer from the form-recognizer studio",
    "Database": "cosmos-database-name for ocr results",
    "Collection": "cosmos-database-collection-name for ocr results",
    "DatabaseFR": "cosmos-database-name for form-recognizer results,
    "CollectionFR": "cosmos-database-collection-name for form-recognizer results",
    "CosmosDBConnection": "CONNECTION STRING TO MY COSMOSDB"
  }
}

```