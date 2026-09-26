using Amazon.S3;
using Amazon.S3.Model;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GraphProcessorAPI.Services.ExternalServices;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace GraphProcessorTest;

public class ObjectStorageServiceTests
{
    private readonly Mock<IAmazonS3> _amazonS3Mock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<ILogger<ObjectStorageService>> _loggerMock = new();

    private readonly IStorageService _storageService;

    public ObjectStorageServiceTests()
    {
        _configurationMock.Setup(x => x["AWS:BucketName"]).Returns("test-bucket");
        _storageService = new ObjectStorageService(_amazonS3Mock.Object, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UploadFileAsync_WithValidFile_ReturnsSuccess()
    {
        // Arrange
        var fileContent = Encoding.UTF8.GetBytes("test file content");
        var stream = new MemoryStream(fileContent);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("test.txt");
        fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
        fileMock.Setup(x => x.ContentType).Returns("text/plain");
        
        _amazonS3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default));

        // Act
        var result = await _storageService.UploadFileAsync(fileMock.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.ObjectKey);
        Assert.Contains("test.txt", result.ObjectKey);
        
        _amazonS3Mock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WithS3Error_ReturnsFailure()
    {
        // Arrange
        var fileContent = Encoding.UTF8.GetBytes("test file content");
        var stream = new MemoryStream(fileContent);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("test.txt");
        fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
        
        _amazonS3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .ThrowsAsync(new AmazonS3Exception("Access Denied"));

        // Act
        var result = await _storageService.UploadFileAsync(fileMock.Object);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Access Denied", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadFileAsync_WithEmptyBucketName_ReturnsObjectKeyWithEmptyPrefix()
    {
        // Arrange
        _configurationMock.Setup(x => x["AWS:BucketName"]).Returns("");
        
        var fileContent = Encoding.UTF8.GetBytes("test file content");
        var stream = new MemoryStream(fileContent);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("test.txt");
        fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
        
        _amazonS3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await _storageService.UploadFileAsync(fileMock.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.ObjectKey);
    }

    [Fact]
    public async Task UploadFileAsync_WithImageContentType_ReturnsSuccess()
    {
        // Arrange
        var fileContent = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        var stream = new MemoryStream(fileContent);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("image.png");
        fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
        fileMock.Setup(x => x.ContentType).Returns("image/png");
        
        _amazonS3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await _storageService.UploadFileAsync(fileMock.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.ObjectKey);
    }

    [Fact]
    public async Task UploadFileAsync_WithEmptyFile_ReturnsSuccess()
    {
        // Arrange
        var stream = new MemoryStream();
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("empty.txt");
        fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
        
        _amazonS3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), default));

        // Act
        var result = await _storageService.UploadFileAsync(fileMock.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.ObjectKey);
    }

    [Fact]
    public async Task GetPrivateFileUrl_WithValidKey_ReturnsPresignedUrl()
    {
        // Arrange
        var expectedUrl = "https://test-bucket.s3.amazonaws.com/25/09/2026/test.txt?signature=abc123";
        
        _amazonS3Mock
            .Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _storageService.GetPrivateFileUrl("25/09/2026/test.txt");

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(expectedUrl, result.PresignedUrlString);
    }

    [Fact]
    public async Task GetPrivateFileUrl_WithS3Error_ReturnsFailure()
    {
        // Arrange
        _amazonS3Mock
            .Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ThrowsAsync(new AmazonS3Exception("NoSuchKey"));

        // Act
        var result = await _storageService.GetPrivateFileUrl("nonexistent/file.txt");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("NoSuchKey", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteFileAsync_WithValidKey_ReturnsSuccess()
    {
        // Arrange
        _amazonS3Mock
            .Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>()))
            .ReturnsAsync(new DeleteObjectResponse());

        // Act
        var result = await _storageService.DeleteFileAsync("25/09/2026/test.txt");

        // Assert
        Assert.True(result.IsValid);
        _amazonS3Mock.Verify(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_WithS3Error_ReturnsFailure()
    {
        // Arrange
        _amazonS3Mock
            .Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .ThrowsAsync(new AmazonS3Exception("AccessDenied"));

        // Act
        var result = await _storageService.DeleteFileAsync("protected/file.txt");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("AccessDenied", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteFileAsync_WithNonExistentFile_ReturnsFailure()
    {
        // Arrange
        _amazonS3Mock
            .Setup(x => x.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>()))
            .ThrowsAsync(new AmazonS3Exception("NoSuchKey"));

        // Act
        var result = await _storageService.DeleteFileAsync("nonexistent/file.txt");

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("NoSuchKey", result.ErrorMessage);
    }

    [Fact]
    public async Task UploadFileAsync_WithSpecialCharactersInFileName_ReturnsSuccess()
    {
        // Arrange
        var fileContent = Encoding.UTF8.GetBytes("test file content");
        var stream = new MemoryStream(fileContent);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(x => x.FileName).Returns("test file (1).txt");
        fileMock.Setup(x => x.OpenReadStream()).Returns(stream);
        fileMock.Setup(x => x.ContentType).Returns("text/plain");
        
        _amazonS3Mock
            .Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()))
            .ReturnsAsync(new PutObjectResponse());

        // Act
        var result = await _storageService.UploadFileAsync(fileMock.Object);

        // Assert
        Assert.True(result.IsValid);
        Assert.NotNull(result.ObjectKey);
        Assert.Contains("test file (1).txt", result.ObjectKey);
    }
}
