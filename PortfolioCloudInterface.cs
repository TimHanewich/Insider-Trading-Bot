using System;
using System.Threading.Tasks;
using SimulatedInvesting;
using Newtonsoft.Json;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Insider_Trading_Bot
{
    public class PortfolioCloudInterface
    {
        public static string GetConnectionString()
        {
            return "";
        }

        public static async Task UploadPortfolioAsync(SimulatedPortfolio sp)
        {
            CloudStorageAccount csa;
            CloudStorageAccount.TryParse(GetConnectionString(), out csa);
            CloudBlobClient cbc = csa.CreateCloudBlobClient();
            CloudBlobContainer cont = cbc.GetContainerReference("general");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference("portfolio");
            await blb.UploadTextAsync(JsonConvert.SerializeObject(sp));
        }

        public static async Task<SimulatedPortfolio> DownloadPortfolioAsync()
        {
            CloudStorageAccount csa;
            CloudStorageAccount.TryParse(GetConnectionString(), out csa);
            CloudBlobClient cbc = csa.CreateCloudBlobClient();
            CloudBlobContainer cont = cbc.GetContainerReference("general");
            await cont.CreateIfNotExistsAsync();
            CloudBlockBlob blb = cont.GetBlockBlobReference("portfolio");
            if (blb.Exists())
            {
                string content = await blb.DownloadTextAsync();
                SimulatedPortfolio ToReturn = JsonConvert.DeserializeObject<SimulatedPortfolio>(content);
                return ToReturn;
            }
            else
            {
                throw new Exception("Portfolio does not exist in the cloud.");
            }
        }
    }
}