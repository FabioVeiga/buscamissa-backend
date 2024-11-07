using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BuscaMissa.Services
{
    public class AzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService()
        {
            _blobServiceClient =  new BlobServiceClient(Environment.GetEnvironmentVariable("BlobStorageConection"));
        }

        public async Task<string> UploadImagemAsync(string base64Image, string pasta, string nomeImagem)
        {
            byte[] imageBytes = Helpers.ImageHelper.ConverterStringEmByte(base64Image);
            var extensao = Helpers.ImageHelper.BuscarExtensao(pasta);
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("buscamissa");
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
            BlobClient blobClient = containerClient.GetBlobClient($"{pasta}/{nomeImagem}{extensao}");
            using (MemoryStream ms = new(imageBytes))
            {
                await blobClient.UploadAsync(ms, overwrite: true);
            }
            return blobClient.Uri.ToString();
        }
    }

}


