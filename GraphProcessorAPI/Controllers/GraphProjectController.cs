using System.Security.Claims;
using System.Text.Json;
using GraphProcessorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GraphProcessorAPI.Repositories;
using GraphProcessorAPI.Services.ExternalServices;

namespace GraphProcessorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GraphProjectController : ControllerBase
{
    private readonly IGraphProjectRepository _projectRepository;
    private readonly ILogger<GraphProjectController> _logger;
    private readonly IStorageService _storageService;

    public GraphProjectController(IGraphProjectRepository projectRepository,  ILogger<GraphProjectController> logger,  IStorageService storageService)
    {
        _projectRepository = projectRepository;
        _logger = logger;
        _storageService = storageService;
    }

    [HttpPost]
    public async Task<IActionResult> PostProject([FromForm] CreateNewProjectDto dto)
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var graphStructure = JsonSerializer.Deserialize<DistanceDataJsonDTO>(dto.GraphStructure);
        
        if (int.TryParse(claimUserId, out int userId) && graphStructure != null)
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

            var storageUploadResult = await _storageService.UploadFileAsync(dto.Image);
            if (storageUploadResult.IsValid)
            {
                await _projectRepository.AddGraphProjectAsync(
                    int.Parse(claimUserId),
                    dto.GraphName,
                    dto.GraphDescription,
                    dto.GraphType,
                    graphStructure.Distances,
                    storageUploadResult.ObjectKey!
                );
                return Ok(new { Message = "Successfully added graph project" });
            }

            return StatusCode(500, "Internal server error(storage result is not valid)");

        } 
        return Unauthorized(new { Error = "Username does not found in http context" });
        
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        
        if (int.TryParse(claimUserId, out int userId))
        {
            List<ProjectViewDto> projectList = await _projectRepository.GetGraphProjectsAsync(userId);
            
            IEnumerable<Task<ProjectViewDto>> tasks = projectList.Select(async project =>
                {
                    var urlGeneratingResult = await _storageService.GetPrivateFileUrl(project.ImageKey);
                    if (urlGeneratingResult.IsValid)
                    {
                        project.ImageKey = urlGeneratingResult.PresignedUrlString ?? string.Empty;
                    }
                    return project;
                });
            var updatedProjectList = await Task.WhenAll(tasks);
            return  Ok(updatedProjectList);
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
            if (existingProject == null)
            {
                return BadRequest(new
                {
                    title = "Project deletion failed",
                    errors = new
                    {
                        Details = new[] {$"Graph with name {graphName} is not found"}
                    }
                });
            }

            await _projectRepository.DeleteGraphProjectAsync(userId, graphName);
            return  Ok(new { Message = "Successfully deleted graph project" });
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }
}