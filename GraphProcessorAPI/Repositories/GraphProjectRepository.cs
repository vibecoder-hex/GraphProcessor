using GraphProcessorAPI.Models;

namespace GraphProcessorAPI.Repositories;

public interface IGraphProjectRepository
{
    Task<Graph?> AddGraphProjectAsync(int userId, string graphName, string graphDescription, GraphType graphType, string graphStructure);
}

public class GraphProjectRepository : IGraphProjectRepository
{
    private readonly GraphProcessorContext _dbContext;

    public GraphProjectRepository(GraphProcessorContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Graph?> AddGraphProjectAsync(int userId, string graphName, string graphDescription,
        GraphType graphType, string graphStructure)
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
        _dbContext.Add(graph);
        await _dbContext.SaveChangesAsync();
        return graph;
    }
}