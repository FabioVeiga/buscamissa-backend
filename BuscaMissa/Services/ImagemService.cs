using Azure.Storage.Blobs;
using BuscaMissa.DTOs.SettingsDto;
using Microsoft.Extensions.Options;

namespace BuscaMissa.Services
{
    public class ImagemService
    {
        private readonly ILogger<ImagemService> _logger;
        private readonly AzureBlobStorage _azureBlobStorage;
        private readonly IConfiguration _configuration;

        public ImagemService(ILogger<ImagemService> logger, IOptions<AzureBlobStorage> optionsAzure, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _azureBlobStorage = optionsAzure.Value;
            AzureBlobStorageStringConnection();
        }

        private void AzureBlobStorageStringConnection()
        {
            if (string.IsNullOrEmpty(_configuration["AzureBlobStorage"]))
            {
                throw new ArgumentNullException("ConnectionString", "Connection string for Azure Blob Storage is not set.");
            }
            _azureBlobStorage.ConnectionString = _configuration["AzureBlobStorage"]!;
        }

        public Uri UploadAzure(string base64Image, string pasta, string nomeArquivo)
        {
            try
            {
                BlobServiceClient blobServiceClient = new(_azureBlobStorage.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_azureBlobStorage.ContainerName);
                containerClient.CreateIfNotExists();
                BlobClient blobClient = containerClient.GetBlobClient(string.Concat(pasta, "/", nomeArquivo));
                var imageBytes = Helpers.ImageHelper.ConverterStringEmByte(base64Image);
                using var stream = new MemoryStream(imageBytes);
                stream.Position = 0;
                blobClient.Upload(stream, overwrite: true);
                return blobClient.Uri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar upload de imagem");
                throw;
            }
        }

        public string ObterUrlAzureBlob(string caminho){
            BlobServiceClient blobServiceClient = new(_azureBlobStorage.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_azureBlobStorage.ContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(caminho);
            return blobClient.Uri.ToString();
        }
    }
}