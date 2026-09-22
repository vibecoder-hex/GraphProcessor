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
            return StatusCode(500, storageUploadResult.ErrorMessage);
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
                    project.ImagePresignedUrl = urlGeneratingResult.IsValid ? urlGeneratingResult.PresignedUrlString : string.Empty;
                    return project;
                });
            var updatedProjectList = await Task.WhenAll(tasks);
            return  Ok(updatedProjectList);
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
            var graphProject = await _projectRepository.GetGraphProjectAsync(userId, graphName);
            if (graphProject == null)
                return NotFound();
            
            var urlGeneratingResult = await _storageService.GetPrivateFileUrl(graphProject.ImageKey);
            if (urlGeneratingResult.IsValid)
            {
                graphProject.ImagePresignedUrl = urlGeneratingResult.PresignedUrlString ?? string.Empty;
                return Ok(graphProject);
            }
            return StatusCode(500, urlGeneratingResult.ErrorMessage);
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProjectByName([FromQuery] string graphName)
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (int.TryParse(claimUserId, out int userId))
        {
            var existingProject = await _projectRepository.GetGraphProjectAsync(userId, graphName);
            if (existingProject == null)
                return NotFound();
            
            var objectDeleteResult = await _storageService.DeleteFileAsync(existingProject.ImageKey);
            if (objectDeleteResult.IsValid)
            {
                await _projectRepository.DeleteGraphProjectAsync(userId, graphName);
                return Ok(new { Message = "Successfully deleted graph project" });
            }
            return StatusCode(500, "Internal server error(storage result is not valid)");
        }
        return Unauthorized(new { Error = "Username does not found in http context" });
    }
}