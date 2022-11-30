using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using System.Text.Json;
using System.Collections.Generic;

namespace CSIntegration
{
    public class FRFunction
    {

        private readonly string csUrl = Environment.GetEnvironmentVariable("CsUrl", EnvironmentVariableTarget.Process);
        private readonly string databaseId = Environment.GetEnvironmentVariable("Database", EnvironmentVariableTarget.Process);
        private readonly string collectionId = Environment.GetEnvironmentVariable("Collection", EnvironmentVariableTarget.Process);
        DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(Environment.GetEnvironmentVariable("CsUrl", EnvironmentVariableTarget.Process)),
            new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("CsAccessKey", EnvironmentVariableTarget.Process)));
        private readonly string modelId = Environment.GetEnvironmentVariable("ModelId", EnvironmentVariableTarget.Process);


        [FunctionName("FormRecognizer")]
        public async Task Run([BlobTrigger("%ContainerFR%/{name}", Connection = "Storage_Landing")] Stream myBlob, string name,
            [CosmosDB(
                databaseName: "%Database%",
                collectionName: "%Collection%",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient cosmosClient,
            ILogger log)
        {
            log.LogInformation($"Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");


            AnalyzeDocumentOperation  operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, modelId, myBlob);
            AnalyzedDocument result = operation.Value.Documents[0]; // Assuming single document coming from blob

            log.LogInformation($"Total confidence '{result.Confidence}'");

            foreach (KeyValuePair<string, DocumentField> kvp in result.Fields)
            {
                if (kvp.Value == null)
                {
                    log.LogInformation($"  Found key with no value: '{kvp.Key}'");
                }
                else
                {
                    log.LogInformation($"  Found key-value pair: '{kvp.Key}' and '{kvp.Value.Content}' with confidence {kvp.Value.Confidence}");
                }
            }

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            var document = new { confidence  = result.Confidence, fields = result.Fields, id = Guid.NewGuid() };
            await cosmosClient.CreateDocumentAsync(collectionUri, document);

        }


    }
    public class FRResultDocument
    {
        public string id { get; set; }
        public float confidence { get; set; }
        KeyValuePair<string, DocumentField> fields { get; set; }
    }
}
