using GraphProcessorAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphProcessorAPI.Repositories;

public interface IGraphProjectRepository
{
    Task<Graph?> AddGraphProjectAsync(int userId, string graphName, string graphDescription, GraphType graphType, Dictionary<string, Dictionary<string, int>> graphStructure);
    Task<List<Graph>?> GetGraphProjectsAsync(int userId);
    Task<Graph?> GetGraphProjectAsync(int userId, string graphName);
    Task DeleteGraphProjectAsync(int userId, string graphName);
}

public class GraphProjectRepository : IGraphProjectRepository
{
    private readonly GraphProcessorContext _dbContext;

    public GraphProjectRepository(GraphProcessorContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Graph?> AddGraphProjectAsync(int userId, string graphName, string graphDescription,
        GraphType graphType, Dictionary<string, Dictionary<string, int>> graphStructure)
    {
        var graph = new Graph()
        {
            Name = graphName,
            Description = graphDescription,
            Type = graphType,
            Creationat = DateTime.UtcNow,
            UserId = userId,
            Structure = graphStructure
        };
        _dbContext.Graphs.Add(graph);
        await _dbContext.SaveChangesAsync();
        return graph;
    }

    public async Task<List<Graph>?> GetGraphProjectsAsync(int userId)
    {
        var graphList = await _dbContext.Graphs
            .Where(graph => graph.UserId == userId)
            .ToListAsync();
        return graphList;
    }

    public async Task<Graph?> GetGraphProjectAsync(int userId, string graphName)
    {
        var graphProject = await _dbContext.Graphs
            .Where(graph => graph.Name == graphName && graph.UserId == userId)
            .FirstOrDefaultAsync();
        return graphProject;
    }

    public async Task DeleteGraphProjectAsync(int userId, string graphName)
    {
        var graphProject = await _dbContext.Graphs
            .Where(graph => graph.UserId == userId)
            .FirstOrDefaultAsync();
        _dbContext.Graphs.Remove(graphProject);
        await _dbContext.SaveChangesAsync();
    }
}