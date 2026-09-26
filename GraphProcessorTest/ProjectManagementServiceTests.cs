using GraphProcessorAPI.Models;
using GraphProcessorAPI.Repositories;
using GraphProcessorAPI.Services;
using GraphProcessorAPI.Services.ExternalServices;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace GraphProcessorTest;

public class ProjectManagementServiceTests
{
    private readonly Mock<IGraphProjectRepository> _projectRepositoryMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();
    
    private readonly IProjectManagementService _projectService;

    public ProjectManagementServiceTests()
    {
        _projectService = new ProjectManagementService(_projectRepositoryMock.Object, _storageServiceMock.Object);
    }

    private const int UserId = 42;
    private const string ObjectKey = "20/09/2026/graph.png";

    private static Dictionary<string, Dictionary<string, int>> BuildStructure() => new()
    {
        ["A"] = new Dictionary<string, int> { { "B", 1 }, { "C", 4 } },
        ["B"] = new Dictionary<string, int> { { "C", 2 } }
    };

    private static ProjectViewDto BuildProject(string name, string imageKey) => new(
        name,
        $"{name} description",
        GraphType.Oriented,
        new DistanceDataJsonDTO(BuildStructure()),
        imageKey,
        new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc));

    private static IFormFile BuildFile(string fileName = "graph.png")
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.OpenReadStream())
            .Returns(() => new MemoryStream(Encoding.UTF8.GetBytes("image bytes")));
        return fileMock.Object;
    }

    #region CreateProject

    [Fact]
    public async Task CreateProject_WhenProjectAlreadyExists_ReturnsFailure()
    {
        var file = BuildFile();

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync(BuildProject("Dijkstra", ObjectKey));

        var result = await _projectService.CreateProject(
            UserId, "Dijkstra", "description", GraphType.Oriented, BuildStructure(), file);

        Assert.False(result.IsValid);
        Assert.Equal("Project already exists", result.ErrorMessage);
        Assert.Null(result.ProjectViewModel);

        _storageServiceMock.Verify(s => s.UploadFileAsync(It.IsAny<IFormFile>()), Times.Never);
        _projectRepositoryMock.Verify(repo => repo.AddGraphProjectAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GraphType>(),
            It.IsAny<Dictionary<string, Dictionary<string, int>>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateProject_WhenUploadSucceeds_ReturnsCreatedProject()
    {
        var file = BuildFile();
        var structure = BuildStructure();
        var createdProject = BuildProject("Dijkstra", ObjectKey);

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync((ProjectViewDto?)null);
        _storageServiceMock.Setup(s => s.UploadFileAsync(file))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true, ObjectKey = ObjectKey });
        _projectRepositoryMock.Setup(repo => repo.AddGraphProjectAsync(
                UserId, "Dijkstra", "description", GraphType.Oriented, structure, ObjectKey))
            .ReturnsAsync(createdProject);

        var result = await _projectService.CreateProject(
            UserId, "Dijkstra", "description", GraphType.Oriented, structure, file);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.ProjectViewModel);
        Assert.Equal("Dijkstra", result.ProjectViewModel.GraphName);
        Assert.Equal(ObjectKey, result.ProjectViewModel.ImageKey);

        _storageServiceMock.Verify(s => s.UploadFileAsync(file), Times.Once);
        _projectRepositoryMock.Verify(repo => repo.AddGraphProjectAsync(
            UserId, "Dijkstra", "description", GraphType.Oriented, structure, ObjectKey), Times.Once);
    }

    [Fact]
    public async Task CreateProject_WhenUploadFails_ReturnsStorageError()
    {
        var file = BuildFile();
        var structure = BuildStructure();

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync((ProjectViewDto?)null);
        _storageServiceMock.Setup(s => s.UploadFileAsync(file))
            .ReturnsAsync(new ObjectStorageResult { IsValid = false, ErrorMessage = "Access Denied" });

        var result = await _projectService.CreateProject(
            UserId, "Dijkstra", "description", GraphType.Oriented, structure, file);

        Assert.False(result.IsValid);
        Assert.Equal("Access Denied", result.ErrorMessage);
        Assert.Null(result.ProjectViewModel);

        _projectRepositoryMock.Verify(repo => repo.AddGraphProjectAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GraphType>(),
            It.IsAny<Dictionary<string, Dictionary<string, int>>>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateProject_WhenUploadReturnsNullObjectKey_PersistsEmptyImageKey()
    {
        var file = BuildFile();
        var structure = BuildStructure();

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync((ProjectViewDto?)null);
        _storageServiceMock.Setup(s => s.UploadFileAsync(file))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true, ObjectKey = null });
        _projectRepositoryMock.Setup(repo => repo.AddGraphProjectAsync(
                UserId, "Dijkstra", "description", GraphType.Oriented, structure, string.Empty))
            .ReturnsAsync(BuildProject("Dijkstra", string.Empty));

        var result = await _projectService.CreateProject(
            UserId, "Dijkstra", "description", GraphType.Oriented, structure, file);

        Assert.True(result.IsValid);
        _projectRepositoryMock.Verify(repo => repo.AddGraphProjectAsync(
            UserId, "Dijkstra", "description", GraphType.Oriented, structure, string.Empty), Times.Once);
    }

    #endregion

    #region DeleteProject

    [Fact]
    public async Task DeleteProject_WhenProjectNotFound_ReturnsFailure()
    {
        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Unknown"))
            .ReturnsAsync((ProjectViewDto?)null);

        var result = await _projectService.DeleteProject(UserId, "Unknown");

        Assert.False(result.IsValid);
        Assert.Equal("Project Unknown", result.ErrorMessage);

        _storageServiceMock.Verify(s => s.DeleteFileAsync(It.IsAny<string>()), Times.Never);
        _projectRepositoryMock.Verify(
            repo => repo.DeleteGraphProjectAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProject_WhenStorageDeleteSucceeds_DeletesProject()
    {
        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync(BuildProject("Dijkstra", ObjectKey));
        _storageServiceMock.Setup(s => s.DeleteFileAsync(ObjectKey))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true });

        var result = await _projectService.DeleteProject(UserId, "Dijkstra");

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);

        _storageServiceMock.Verify(s => s.DeleteFileAsync(ObjectKey), Times.Once);
        _projectRepositoryMock.Verify(repo => repo.DeleteGraphProjectAsync(UserId, "Dijkstra"), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_WhenStorageDeleteFails_KeepsProject()
    {
        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync(BuildProject("Dijkstra", ObjectKey));
        _storageServiceMock.Setup(s => s.DeleteFileAsync(ObjectKey))
            .ReturnsAsync(new ObjectStorageResult { IsValid = false, ErrorMessage = "NoSuchKey" });

        var result = await _projectService.DeleteProject(UserId, "Dijkstra");

        Assert.False(result.IsValid);
        Assert.Equal("NoSuchKey", result.ErrorMessage);

        _projectRepositoryMock.Verify(
            repo => repo.DeleteGraphProjectAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region GetProjects

    [Fact]
    public async Task GetProjects_WhenUserHasNoProjects_ReturnsEmptySuccess()
    {
        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectsAsync(UserId))
            .ReturnsAsync(new List<ProjectViewDto>());

        var result = await _projectService.GetProjects(UserId);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Projects);
        Assert.Empty(result.Projects);

        _storageServiceMock.Verify(s => s.GetPrivateFileUrl(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetProjects_WhenAllUrlsResolved_ReturnsProjectsWithPresignedUrls()
    {
        var first = BuildProject("Dijkstra", "20/09/2026/first.png");
        var second = BuildProject("Bfs", "20/09/2026/second.png");

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectsAsync(UserId))
            .ReturnsAsync(new List<ProjectViewDto> { first, second });
        _storageServiceMock.Setup(s => s.GetPrivateFileUrl("20/09/2026/first.png"))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true, PresignedUrlString = "https://s3/first?sig=1" });
        _storageServiceMock.Setup(s => s.GetPrivateFileUrl("20/09/2026/second.png"))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true, PresignedUrlString = "https://s3/second?sig=2" });

        var result = await _projectService.GetProjects(UserId);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Projects);
        Assert.Equal(2, result.Projects.Count);
        Assert.Equal("https://s3/first?sig=1", result.Projects[0].ImagePresignedUrl);
        Assert.Equal("https://s3/second?sig=2", result.Projects[1].ImagePresignedUrl);

        _storageServiceMock.Verify(s => s.GetPrivateFileUrl("20/09/2026/first.png"), Times.Once);
        _storageServiceMock.Verify(s => s.GetPrivateFileUrl("20/09/2026/second.png"), Times.Once);
    }

    [Fact]
    public async Task GetProjects_WhenOneUrlFails_ReturnsStorageError()
    {
        var first = BuildProject("Dijkstra", "20/09/2026/first.png");
        var second = BuildProject("Bfs", "20/09/2026/second.png");

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectsAsync(UserId))
            .ReturnsAsync(new List<ProjectViewDto> { first, second });
        _storageServiceMock.Setup(s => s.GetPrivateFileUrl("20/09/2026/first.png"))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true, PresignedUrlString = "https://s3/first?sig=1" });
        _storageServiceMock.Setup(s => s.GetPrivateFileUrl("20/09/2026/second.png"))
            .ReturnsAsync(new ObjectStorageResult { IsValid = false, ErrorMessage = "NoSuchKey" });

        var result = await _projectService.GetProjects(UserId);

        Assert.False(result.IsValid);
        Assert.Equal("NoSuchKey", result.ErrorMessage);
        Assert.Null(result.Projects);
    }

    [Fact]
    public async Task GetProjects_WhenUrlIsNull_FallsBackToEmptyString()
    {
        var project = BuildProject("Dijkstra", "20/09/2026/first.png");

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectsAsync(UserId))
            .ReturnsAsync(new List<ProjectViewDto> { project });
        _storageServiceMock.Setup(s => s.GetPrivateFileUrl("20/09/2026/first.png"))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true, PresignedUrlString = null });

        var result = await _projectService.GetProjects(UserId);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Projects);
        Assert.Equal(string.Empty, result.Projects[0].ImagePresignedUrl);
    }

    #endregion

    #region GetSelectedProject

    [Fact]
    public async Task GetSelectedProject_WhenProjectNotFound_ReturnsFailure()
    {
        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Unknown"))
            .ReturnsAsync((ProjectViewDto?)null);

        var result = await _projectService.GetSelectedProject(UserId, "Unknown");

        Assert.False(result.IsValid);
        Assert.Equal("Project does not exists", result.ErrorMessage);
        Assert.Null(result.ProjectViewModel);

        _storageServiceMock.Verify(s => s.GetPrivateFileUrl(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetSelectedProject_WhenUrlResolved_ReturnsProjectWithPresignedUrl()
    {
        var project = BuildProject("Dijkstra", ObjectKey);

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync(project);
        _storageServiceMock.Setup(s => s.GetPrivateFileUrl(ObjectKey))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true, PresignedUrlString = "https://s3/graph?sig=1" });

        var result = await _projectService.GetSelectedProject(UserId, "Dijkstra");

        Assert.True(result.IsValid);
        Assert.NotNull(result.ProjectViewModel);
        Assert.Equal("Dijkstra", result.ProjectViewModel.GraphName);
        Assert.Equal("https://s3/graph?sig=1", result.ProjectViewModel.ImagePresignedUrl);
        Assert.Equal(ObjectKey, result.ProjectViewModel.ImageKey);

        _storageServiceMock.Verify(s => s.GetPrivateFileUrl(ObjectKey), Times.Once);
    }

    [Fact]
    public async Task GetSelectedProject_WhenUrlFails_ReturnsStorageError()
    {
        var project = BuildProject("Dijkstra", ObjectKey);

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync(project);
        _storageServiceMock.Setup(s => s.GetPrivateFileUrl(ObjectKey))
            .ReturnsAsync(new ObjectStorageResult { IsValid = false, ErrorMessage = "Access Denied" });

        var result = await _projectService.GetSelectedProject(UserId, "Dijkstra");

        Assert.False(result.IsValid);
        Assert.Equal("Access Denied", result.ErrorMessage);
        Assert.Null(result.ProjectViewModel);
    }

    [Fact]
    public async Task GetSelectedProject_WhenUrlIsNull_FallsBackToEmptyString()
    {
        var project = BuildProject("Dijkstra", ObjectKey);

        _projectRepositoryMock.Setup(repo => repo.GetGraphProjectAsync(UserId, "Dijkstra"))
            .ReturnsAsync(project);
        _storageServiceMock.Setup(s => s.GetPrivateFileUrl(ObjectKey))
            .ReturnsAsync(new ObjectStorageResult { IsValid = true, PresignedUrlString = null });

        var result = await _projectService.GetSelectedProject(UserId, "Dijkstra");

        Assert.True(result.IsValid);
        Assert.NotNull(result.ProjectViewModel);
        Assert.Equal(string.Empty, result.ProjectViewModel.ImagePresignedUrl);
    }

    #endregion
}
