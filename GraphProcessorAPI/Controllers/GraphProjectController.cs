using System.Security.Claims;
using System.Text.Json;
using GraphProcessorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GraphProcessorAPI.Services;

namespace GraphProcessorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GraphProjectController : ControllerBase
{
    private readonly IProjectManagementService _projectService;
    private readonly ILogger<GraphProjectController> _logger;

    public GraphProjectController(IProjectManagementService projectService, ILogger<GraphProjectController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostProject([FromForm] CreateNewProjectDto dto)
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var graphStructure = JsonSerializer.Deserialize<DistanceDataJsonDTO>(dto.GraphStructure);
        
        if (int.TryParse(claimUserId, out int userId) && graphStructure != null)
        {
            var createProjectResult = await _projectService.CreateProject(
                userId,
                dto.GraphName,
                dto.GraphDescription,
                dto.GraphType,
                graphStructure.Distances,
                dto.Image
                );
            
            if (createProjectResult.IsValid)
                return Ok(new { Message = "Successfully created graph project" });
            
            return BadRequest(new
            {
                title = "Project creation failed",
                errors = new
                {
                    Details = new[] { createProjectResult.ErrorMessage }
                }
            });
        } 
        return Unauthorized(new { Error = "Username does not found in http context" });
        
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        
        if (int.TryParse(claimUserId, out int userId))
        {
            var projectGetResult = await _projectService.GetProjects(userId);
            if (projectGetResult.IsValid)
                return Ok(projectGetResult.Projects);
            return BadRequest(new
            {
                title = "Project getting failed",
                errors = new
                {
                    Details = new[] { projectGetResult.ErrorMessage }
                }
            });
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }

    [HttpGet("Selected")]
    public async Task<IActionResult> GetProjectByName([FromQuery] string graphName)
    {
        _logger.LogInformation(graphName);
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (int.TryParse(claimUserId, out int userId))
        {
            var projectGetResult = await _projectService.GetSelectedProject(userId, graphName);
            if (projectGetResult.IsValid)
                return Ok(projectGetResult.ProjectViewModel);
            
            return BadRequest(new
            {
                title = "Project getting failed",
                errors = new
                {
                    Details = new[] { projectGetResult.ErrorMessage }
                }
            });
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProjectByName([FromQuery] string graphName)
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (int.TryParse(claimUserId, out int userId))
        {
            var projectDeletionResult = await _projectService.DeleteProject(userId, graphName);
            if (projectDeletionResult.IsValid)
                return Ok(new {Message = "Successfully deleted graph project"});
            
            return BadRequest(new
            {
                title = "Project getting failed",
                errors = new
                {
                    Details = new[] { projectDeletionResult.ErrorMessage }
                }
            });
            
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }
}