using GraphProcessorAPI.Models;
using GraphProcessorAPI.Repositories;
using GraphProcessorAPI.Services.ExternalServices;

namespace GraphProcessorAPI.Services;

public interface IProjectManagementService
{
    Task<ProjectManagementResult> CreateProject(int userId, string title, string description, GraphType type, 
        Dictionary<string, Dictionary<string, int>> structure, IFormFile file);
    Task<ProjectManagementResult> DeleteProject(int userId, string graphName);
    Task<ProjectManagementResult> GetProjects(int userId);
    Task<ProjectManagementResult> GetSelectedProject(int userId, string graphName);
}

public class ProjectManagementService : IProjectManagementService
{
    private readonly IGraphProjectRepository _projectRepository;
    private readonly IStorageService _storageService;
    
    public ProjectManagementService(IGraphProjectRepository projectRepository,  IStorageService storageService)
    {
        _projectRepository = projectRepository;
        _storageService = storageService;
    }

    public async Task<ProjectManagementResult> CreateProject(int userId, string title, string description, GraphType type, 
        Dictionary<string, Dictionary<string, int>> structure, IFormFile file)
    {
        var existingProject = await _projectRepository.GetGraphProjectAsync(userId, title);
        if (existingProject != null)
            return new ProjectManagementResult { IsValid = false, ErrorMessage = "Project already exists" };

        var objectUploadResult = await _storageService.UploadFileAsync(file);
        if (objectUploadResult.IsValid)
        { 
            string objectKey = objectUploadResult.ObjectKey ?? ""; 
            var newProject = await _projectRepository.AddGraphProjectAsync(userId, title, description, type, structure, objectKey);
            return new ProjectManagementResult { IsValid = true, ProjectViewModel = newProject};
        }
        return new ProjectManagementResult { IsValid = false, ErrorMessage = objectUploadResult.ErrorMessage };
    }

    public async Task<ProjectManagementResult> DeleteProject(int userId, string graphName)
    {
        var existingProject = await _projectRepository.GetGraphProjectAsync(userId, graphName);
        if (existingProject == null)
            return new ProjectManagementResult { IsValid = false, ErrorMessage = $"Project {graphName}" };

        var objectDeleteResult = await _storageService.DeleteFileAsync(existingProject.ImageKey);
        if (objectDeleteResult.IsValid)
        {
            await _projectRepository.DeleteGraphProjectAsync(userId, graphName);
            return new ProjectManagementResult { IsValid = true };
        }
        return new ProjectManagementResult { IsValid = false, ErrorMessage = objectDeleteResult.ErrorMessage };
    }

    public async Task<ProjectManagementResult> GetProjects(int userId)
    {
        var projectList = await _projectRepository.GetGraphProjectsAsync(userId);
        if (projectList.Count > 0)
        {
            var tasks = projectList
                .Select(async project =>
                {
                    var objectUploadResult = await _storageService.GetPrivateFileUrl(project.ImageKey);
                    return new { Project = project, Result = objectUploadResult } ;
                })
                .ToList();
            var completedResults = await Task.WhenAll(tasks);

            foreach (var result in completedResults)
            {
                if (!result.Result.IsValid)
                    return new ProjectManagementResult {IsValid = false, ErrorMessage = result.Result.ErrorMessage };
                
                result.Project.ImagePresignedUrl = result.Result.PresignedUrlString ?? string.Empty;
            }
        }
        return new ProjectManagementResult { IsValid = true, Projects = projectList };
    }

    public async Task<ProjectManagementResult> GetSelectedProject(int userId, string graphName)
    {
        var project = await _projectRepository.GetGraphProjectAsync(userId, graphName);
        if (project == null)
            return new ProjectManagementResult { IsValid = false, ErrorMessage = "Project does not exists" };

        var objectUploadResult = await _storageService.GetPrivateFileUrl(project.ImageKey);
        if (objectUploadResult.IsValid)
        {
            project.ImagePresignedUrl = objectUploadResult.PresignedUrlString ?? string.Empty;
            return new ProjectManagementResult { IsValid = true, ProjectViewModel = project };
        }
        return new ProjectManagementResult { IsValid = false, ErrorMessage = objectUploadResult.ErrorMessage };
    }
}