using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.Documents.Client;

namespace CSIntegration
{
    public class OcrFunction
    {
        private readonly string csUrl = Environment.GetEnvironmentVariable("CsUrl", EnvironmentVariableTarget.Process);
        private readonly string databaseId = Environment.GetEnvironmentVariable("Database", EnvironmentVariableTarget.Process);
        private readonly string collectionId = Environment.GetEnvironmentVariable("Collection", EnvironmentVariableTarget.Process);

        ComputerVisionClient client =
             new ComputerVisionClient(new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("CsAccessKey", EnvironmentVariableTarget.Process)))
             { Endpoint = Environment.GetEnvironmentVariable("CsUrl", EnvironmentVariableTarget.Process) };

        private Regex regex;

        public OcrFunction()
        {
            var re = Environment.GetEnvironmentVariable("RegExpMatcher", EnvironmentVariableTarget.Process);
            if (! string.IsNullOrEmpty(re)) {
                regex = new Regex(re);
            }
        }


        [FunctionName("OCR")]
        public async Task Run([BlobTrigger("%container%/{name}", Connection = "Storage_Landing")]Stream myBlob, string name,
            [CosmosDB(
                databaseName: "%Database%",
                collectionName: "%Collection%",
                ConnectionStringSetting = "CosmosDBConnection")] DocumentClient cosmosClient,
            ILogger log)
        {
            log.LogInformation($"Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var request = new HttpRequestMessage(HttpMethod.Post, csUrl);


            var operationHeaders = await client.ReadInStreamAsync(myBlob);

            ReadOperationResult results;
            do
            {
                results = await client.GetReadResultAsync(operationHeaders.GetGuid());
            }
            while ((results.Status == OperationStatusCodes.Running ||
               results.Status == OperationStatusCodes.NotStarted));


            var texts = results.AnalyzeResult.ReadResults.SelectMany(r => r.Lines).Select(l => l.Text).ToList();
            if (regex != null)
            { 
                texts = texts.Where(t => !regex.IsMatch(t)).ToList();
            }

            if (texts.Count() > 0)
            {
                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
                var document = new { Lines = texts, id = Guid.NewGuid() };
                await cosmosClient.CreateDocumentAsync(collectionUri, document);
            } else
            {
                log.LogInformation($"No text was read that would have passed filtering for {name}");
            }

        }
    }

    public class OcrResultDocument
    {
        public string id { get; set; }
        public string[] Lines { get; set; }
    }
}
