using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.IO;
using System.Linq;


namespace CSIntegration
{
    public class PDFSplitFunction
    {

        [FunctionName("PdfSplitter")]
        public void Run([BlobTrigger("%PdfSplitter%/{name}.pdf", Connection = "Storage_Landing")] Stream inputBlob, string name,

           [Blob("%PdfSplitterOutput%/{name}_page1.pdf", FileAccess.Write, Connection = "Storage_Landing")] BlobClient client1,
           [Blob("%PdfSplitterOutput%/{name}_rest.pdf", FileAccess.Write, Connection = "Storage_Landing")] BlobClient client2,
           ILogger log)
        {

            log.LogInformation($"Blob trigger PdfSplitter Processed blob\n Name:{name} \n Size: {inputBlob.Length} Bytes {client1.BlobContainerName}");

            // Check that we don't trigger from PDF changes if output is in same directory as input
            if (new string[] { "_page1", "_rest" }.Any(name.Contains))
            {
                return;
            }

            var inputDocument = PdfReader.Open(inputBlob, PdfDocumentOpenMode.Import);
            var frontPage = new PdfDocument();
            var tailPages = new PdfDocument();
            int count = inputDocument.PageCount;
            if (count > 0)
            {
                frontPage.AddPage(inputDocument.Pages[0]);

            }
            for (int idx = 1; idx < count; idx++)
            {
                tailPages.AddPage(inputDocument.Pages[idx]);

            }

                        
            using (var stream1 = new MemoryStream())
            {
                frontPage.Save(stream1);            
                stream1.Flush();
                client1.Upload(stream1);
            }
            
            using (var stream2 = new MemoryStream())
            {
                tailPages.Save(stream2);
                stream2.Flush();
                client2.Upload(stream2);
            }            
        }
    }
}
