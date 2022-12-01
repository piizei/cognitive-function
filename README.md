# cognitive-function
Azure Functions integration with Cognitive Services

This function scans for a blob-storage container (specified by env variable ```Container```) for images and send them over to Cognitive Services for Read-In.

The result is filtered by RegExp provided in env variable ```RegExpMatcher```

The filtered result is loaded in CosmosDB database provided in env variable ```Database``` and Collection ```Collection```

The cognitive services instance is configured to env variables ```CsUrl``` and  ```CsAccessKey```


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
    "ModelId": "ID of the trained model of form-recognizer from the form-recognizer studio",
    "Database": "cosmos-database-name for ocr results",
    "Collection": "cosmos-database-collection-name for ocr results",
    "DatabaseFr": "cosmos-database-name for form-recognizer results,
    "CollectionFr": "cosmos-database-collection-name for form-recognizer results",
    "CosmosDBConnection": "CONNECTION STRING TO MY COSMOSDB"
  }
}

```