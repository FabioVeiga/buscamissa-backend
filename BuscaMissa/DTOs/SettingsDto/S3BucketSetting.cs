using Amazon;

namespace BuscaMissa.DTOs.SettingsDto
{
    public class S3BucketSetting
    {
        public string AwsAccessKeyId { get; set; } = default!;
        public string AwsSecretAccessKey { get; set; } = default!;
        public string BucketName { get; set; } = default!;
        public RegionEndpoint RegionDefault { get; set; } = RegionEndpoint.USEast1;
    }
}