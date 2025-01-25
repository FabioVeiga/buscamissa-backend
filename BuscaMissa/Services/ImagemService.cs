using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace BuscaMissa.Services
{
    public class ImagemService
    {
        private readonly ILogger<ImagemService> _logger;
        private readonly AmazonS3Client _s3Client;
        private const string _bucketName = "buscamissadev";
        public ImagemService(ILogger<ImagemService> logger)
        {
            _logger = logger;
            _s3Client = new AmazonS3Client("AKIAZOZQFTWQU2KVC45E", "dwNpEfQheWodfbrJgo0/tizDyONoAhU/X7ULAZvh", RegionEndpoint.USEast1);
        }

        public async Task<string> UploadAsync(string base64Image, string pasta, string nomeArquivo, string extensao){
            try
            {
                var imageBytes = Helpers.ImageHelper.ConverterStringEmByte(base64Image);
                var key = $"{pasta}/{nomeArquivo}";
                using var stream = new MemoryStream(imageBytes);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    Key = key,
                    BucketName = _bucketName,
                    ContentType = $"image/{extensao}",
                };
                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);

                return ObterPreVisualizacao(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar upload de imagem");
                throw;
            }
        }

        public string ObterPreVisualizacao(string key){
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddHours(1)
                };
                return _s3Client.GetPreSignedURL(request);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pré-visualização de imagem");
                throw;
            }
        }
    }
}