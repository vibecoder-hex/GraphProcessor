using Amazon.S3;
using Amazon.S3.Model;
using GraphProcessorAPI.Models;

namespace GraphProcessorAPI.Services.ExternalServices;

public interface IStorageService
{
    Task<ObjectStorageResult> UploadFileAsync(IFormFile file);
    Task<ObjectStorageResult> GetPrivateFileUrl(string objectKey);
}

public class ObjectStorageService : IStorageService
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

    public async Task<ObjectStorageResult> UploadFileAsync(IFormFile file)
    {
        string formatedDate = DateTime.Now.ToString("dd\\/MM\\/yyyy");
        string objectKey = $"{formatedDate}/{file.FileName}";
        await using (var stream = file.OpenReadStream())
        {
            var objectRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                ContentType = file.ContentType,
                InputStream = stream
            };
            try
            {
                await _s3Client.PutObjectAsync(objectRequest);
                return new ObjectStorageResult { IsValid = true, ObjectKey = objectKey };
            }
            catch (AmazonS3Exception ex)
            {
                return new ObjectStorageResult { IsValid = false, ErrorMessage = ex.Message };
            }
        }
    }

    public async Task<ObjectStorageResult> GetPrivateFileUrl(string objectKey)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddHours(2)
        };
        try
        {
            string fileUrl = await _s3Client.GetPreSignedURLAsync(request);
            return new ObjectStorageResult { IsValid = true, PresignedUrlString = fileUrl };
        }
        catch (AmazonS3Exception ex)
        {
            return new ObjectStorageResult { IsValid = false, ErrorMessage = ex.Message };
        }
    }
}