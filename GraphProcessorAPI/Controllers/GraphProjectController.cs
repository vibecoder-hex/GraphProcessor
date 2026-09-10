using System.Security.Claims;
using GraphProcessorAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GraphProcessorAPI.Repositories;

namespace GraphProcessorAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class GraphProjectController : ControllerBase
{
    private readonly IGraphProjectRepository _projectRepository;

    public GraphProjectController(IGraphProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    [HttpPost]
    public async Task<IActionResult> PostProject([FromBody] CreateNewProjectDto dto)
    {
        string claimUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        await _projectRepository.AddGraphProjectAsync(
            int.Parse(claimUserId),
            dto.GraphName,
            dto.GraphDescription,
            dto.GraphType,
            dto.GraphStructure
        );
        return Ok(new { Message = "Successfully added graph project" });
    }
}