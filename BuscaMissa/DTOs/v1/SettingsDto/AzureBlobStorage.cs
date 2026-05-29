namespace BuscaMissa.DTOs.SettingsDto
{
    public class AzureBlobStorage
    {
        public string ConnectionString { get; set; } = default!;
        public string ContainerName { get; set; } = default!;
        public string BlobName { get; set; } = default!;
        public string BaseUri { get; set; } = default!;
    }
}