using Amazon.S3;
using Amazon.S3.Model;

namespace GraphProcessorAPI.Services.ExternalServices;

public interface IObjectStorageService
{
    Task UploadFileAsync(IFormFile file);
}

public class ObjectStorageService : IObjectStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ObjectStorageService> _logger;
    private readonly string _bucketName;
    
    public ObjectStorageService(IAmazonS3 s3Client,  IConfiguration configuration, ILogger<ObjectStorageService> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:BucketName"] ?? "";
    }

    public async Task UploadFileAsync(IFormFile file)
    {
        string formatedDate = DateTime.Now.ToString("dd\\/MM\\/yyyy");
        _logger.LogInformation($"Uploading file to bucket {_bucketName}/{formatedDate}...");
        await using (var stream = file.OpenReadStream())
        {
            var objectRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = $"{formatedDate}/{file.FileName}",
                ContentType = file.ContentType,
                InputStream = stream
            };
            await _s3Client.PutObjectAsync(objectRequest);
        }
    }
}