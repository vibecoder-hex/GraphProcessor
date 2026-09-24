using GraphProcessorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphProcessorAPI.Repositories;

public interface IGraphProjectRepository
{
    Task<ProjectViewDto?> AddGraphProjectAsync(int userId, string graphName, string graphDescription, GraphType graphType, Dictionary<string, Dictionary<string, int>> graphStructure, string imageFilename);
    Task<List<ProjectViewDto>> GetGraphProjectsAsync(int userId);
    Task<ProjectViewDto?> GetGraphProjectAsync(int userId, string graphName);
    Task DeleteGraphProjectAsync(int userId, string graphName);
}

public class GraphProjectRepository : IGraphProjectRepository
{
    private readonly GraphProcessorContext _dbContext;

    public GraphProjectRepository(GraphProcessorContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectViewDto?> AddGraphProjectAsync(int userId, string graphName, string graphDescription, 
        GraphType graphType, Dictionary<string, Dictionary<string, int>> graphStructure, string imageFilename)
    {
        var graph = new Graph()
        {
            Name = graphName,
            Description = graphDescription,
            Type = graphType,
            Creationat = DateTime.UtcNow,
            UserId = userId,
            Structure = graphStructure,
            Image = imageFilename
        };
        _dbContext.Graphs.Add(graph);
        await _dbContext.SaveChangesAsync();
        return new ProjectViewDto(graph.Name, graph.Description, graph.Type, new DistanceDataJsonDTO(graph.Structure), graph.Image, graph.Creationat);
    }

    public async Task<List<ProjectViewDto>> GetGraphProjectsAsync(int userId)
    {
        return await _dbContext.Graphs
            .AsNoTracking()
            .Where(graph => graph.UserId == userId)
            .Select(graph => new ProjectViewDto(
                graph.Name,
                graph.Description,
                graph.Type,
                new DistanceDataJsonDTO(graph.Structure),
                graph.Image,
                graph.Creationat))
            .ToListAsync();
    }

    public async Task<ProjectViewDto?> GetGraphProjectAsync(int userId, string graphName)
    {
        return await _dbContext.Graphs
            .AsNoTracking()
            .Where(graph => graph.Name == graphName && graph.UserId == userId)
            .Select(graph => new ProjectViewDto(
                graph.Name, 
                graph.Description,
                graph.Type,
                new DistanceDataJsonDTO(graph.Structure),
                graph.Image,
                graph.Creationat))
            .FirstOrDefaultAsync();
    }

    public async Task DeleteGraphProjectAsync(int userId, string graphName)
    {
        var graphProject = await _dbContext.Graphs
            .Where(graph => graph.UserId == userId  && graph.Name == graphName)
            .FirstOrDefaultAsync();
        _dbContext.Graphs.Remove(graphProject);
        await _dbContext.SaveChangesAsync();
    }
}