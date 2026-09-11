using System.Security.Claims;
using GraphProcessorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GraphProcessorAPI.Repositories;

namespace GraphProcessorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GraphProjectController : ControllerBase
{
    private readonly IGraphProjectRepository _projectRepository;
    private readonly ILogger<GraphProjectController> _logger;

    public GraphProjectController(IGraphProjectRepository projectRepository,  ILogger<GraphProjectController> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostProject([FromBody] CreateNewProjectDto dto)
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (int.TryParse(claimUserId, out int userId))
        {
            var existingProject = await _projectRepository.GetGraphProjectAsync(userId, dto.GraphName);
            if (existingProject != null)
            {
                return BadRequest(new
                {
                    title = "Project creation failed",
                    errors = new
                    {
                        Details = new[] {$"Graph with name {dto.GraphName} already exists"}
                    }
                });
            }
            await _projectRepository.AddGraphProjectAsync(
                int.Parse(claimUserId),
                dto.GraphName,
                dto.GraphDescription,
                dto.GraphType,
                dto.GraphStructure.Distances
            );
            return Ok(new { Message = "Successfully added graph project" });
        } 
        return Unauthorized(new { Error = "Username does not found in http context" });
        
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (int.TryParse(claimUserId, out int userId))
        {
            List<Graph>? graphList = await _projectRepository.GetGraphProjectsAsync(userId);
            return Ok(graphList);
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }

    [HttpGet("graphName")]
    public async Task<IActionResult> GetProjectByName(string graphName)
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (int.TryParse(claimUserId, out int userId))
        {
            var graphProject = await _projectRepository.GetGraphProjectAsync(userId, graphName);
            if (graphProject == null)
                return NotFound();
            return Ok(graphProject);
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }

    [HttpDelete("graphName")]
    public async Task<IActionResult> DeleteProjectByName(string graphName)
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (int.TryParse(claimUserId, out int userId))
        {
            var existingProject = await _projectRepository.GetGraphProjectAsync(userId, graphName);
            if (existingProject != null)
            {
                return BadRequest(new
                {
                    title = "Project deletion failed",
                    errors = new
                    {
                        Details = new[] {$"Graph with name {graphName} already exists"}
                    }
                });
            }

            await _projectRepository.DeleteGraphProjectAsync(userId, graphName);
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }
}