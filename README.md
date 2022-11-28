# cognitive-function
Azure Functions integration with Cognitive Services

This function scans for a blob-storage container (specified by env variable ```Container```) for images and send them over to Cognitive Services for Read-In.

The result is filtered by RegExp provided in env variable ```RegExpMatcher```

The filtered result is loaded in CosmosDB database provided in env variable ```Database``` and Collection ```Collection```

The cognitive services instance is configured to env variables ```CsUrl``` and  ```CsAccessKey```
