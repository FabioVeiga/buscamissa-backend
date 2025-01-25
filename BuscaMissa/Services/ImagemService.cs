using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BuscaMissa.DTOs.SettingsDto;
using Microsoft.Extensions.Options;

namespace BuscaMissa.Services
{
    public class ImagemService
    {
        private readonly ILogger<ImagemService> _logger;
        private readonly AmazonS3Client _s3Client;
        private readonly S3BucketSetting _s3BucketSetting;
        
        public ImagemService(ILogger<ImagemService> logger, IOptions<S3BucketSetting> options)
        {
            _logger = logger;
            _s3BucketSetting = options.Value;
            _s3Client = new AmazonS3Client(_s3BucketSetting.AwsAccessKeyId, _s3BucketSetting.AwsSecretAccessKey, _s3BucketSetting.RegionDefault);
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
                    BucketName = _s3BucketSetting.BucketName,
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
                    BucketName = _s3BucketSetting.BucketName,
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