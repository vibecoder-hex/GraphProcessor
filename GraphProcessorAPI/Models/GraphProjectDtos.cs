namespace GraphProcessorAPI.Models;

public record CreateNewProjectDto(string GraphName, string GraphDescription, GraphType GraphType, DistanceDataJsonDTO GraphStructure);