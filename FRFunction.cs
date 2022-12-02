using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using System.Text.Json;
using System.Collections.Generic;


namespace CSIntegration
{
    public class FRFunction
    {
        DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(Environment.GetEnvironmentVariable("CsUrl", EnvironmentVariableTarget.Process)),
            new Azure.AzureKeyCredential(Environment.GetEnvironmentVariable("CsAccessKey", EnvironmentVariableTarget.Process)));
        private readonly string modelId = Environment.GetEnvironmentVariable("ModelId", EnvironmentVariableTarget.Process);


        // Triggers on PDF only
        [FunctionName("FormRecognizer")]
        public void Run([BlobTrigger("%ContainerFR%/{name}.pdf", Connection = "Storage_Landing")] Stream inputBlob, string name,
            [CosmosDB(
                databaseName: "%DatabaseFR%",
                collectionName: "%CollectionFR%",
                ConnectionStringSetting = "CosmosDBConnection")] out dynamic document,
            [Blob("%ContainerFR%/{name}.json", Connection = "Storage_Landing")] out string outputBlob,
            ILogger log)
        {
            log.LogInformation($"Blob trigger function Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes");


            AnalyzeDocumentOperation  operation = client.AnalyzeDocument(WaitUntil.Completed, modelId, inputBlob);
            AnalyzedDocument result = operation.Value.Documents[0]; // Assuming single document coming from blob

            //Write all analyzation results to json file in blob storage            
            outputBlob = JsonSerializer.Serialize(operation.Value);

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
            //Write to CosmosDb
            document = new { confidence  = result.Confidence, fields = result.Fields, id = Guid.NewGuid() };            

        }


    }
    public class FRResultDocument
    {
        public string id { get; set; }
        public float confidence { get; set; }
        KeyValuePair<string, DocumentField> fields { get; set; }
    }
}
