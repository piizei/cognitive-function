using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSIntegration
{
    public static class Extensions
    {
        public static Guid GetGuid(this ReadInStreamHeaders streamHeaders) 
        {
            string operationLocation = streamHeaders.OperationLocation;
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);
            return Guid.Parse(operationId);
        }

    }
}
