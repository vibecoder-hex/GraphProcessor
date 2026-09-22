namespace GraphProcessorAPI.Models;

public record CreateNewProjectDto(string GraphName, string GraphDescription, GraphType GraphType, string GraphStructure, IFormFile Image);

public record ProjectViewDto(
    string GraphName,
    string GraphDescription,
    GraphType GraphType,
    DistanceDataJsonDTO GraphStructure,
    string ImageKey,
    DateTime CreatedAt)
{
    public string ImagePresignedUrl { get; set; } = ImageKey;
};