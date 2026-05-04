using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Projects.CreateProject;
using TaskFlow.Features.Projects.DeleteProject;
using TaskFlow.Features.Projects.GetProjects;
using TaskFlow.Features.Projects.UpdateProject;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.Api.UnitTests.Features.Projects;

public class ProjectsTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly string _userId = "user-projects-test";

    public ProjectsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    // ── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProject_ShouldReturnDto_WithCorrectFields()
    {
        var handler = new CreateProjectHandler(_dbContext);
        var command = new CreateProjectCommand
        {
            Name = "My Board",
            Description = "A test board",
            Color = "#4F46E5",
            UserId = _userId
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("My Board");
        result.Color.Should().Be("#4F46E5");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateProject_ShouldPersistToDatabase()
    {
        var handler = new CreateProjectHandler(_dbContext);
        var command = new CreateProjectCommand { Name = "Persisted", UserId = _userId };

        await handler.Handle(command, CancellationToken.None);

        var saved = await _dbContext.Projects.FirstOrDefaultAsync(p => p.UserId == _userId);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Persisted");
    }

    // ── Get ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProjects_ShouldReturnOnlyUserProjects()
    {
        _dbContext.Projects.AddRange(
            new Project { Id = Guid.NewGuid(), Name = "Mine", UserId = _userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Project { Id = Guid.NewGuid(), Name = "Other", UserId = "other-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await _dbContext.SaveChangesAsync();

        var handler = new GetProjectsHandler(_dbContext);
        var result = await handler.Handle(new GetProjectsQuery { UserId = _userId }, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Mine");
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProject_ShouldChangeName()
    {
        var project = new Project { Id = Guid.NewGuid(), Name = "Old Name", UserId = _userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        var handler = new UpdateProjectHandler(_dbContext);
        var result = await handler.Handle(new UpdateProjectCommand
        {
            ProjectId = project.Id,
            Name = "New Name",
            UserId = _userId
        }, CancellationToken.None);

        result.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateProject_ShouldThrow_WhenNotFound()
    {
        var handler = new UpdateProjectHandler(_dbContext);
        await handler.Invoking(h => h.Handle(new UpdateProjectCommand
        {
            ProjectId = Guid.NewGuid(),
            Name = "X",
            UserId = _userId
        }, CancellationToken.None))
        .Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteProject_ShouldRemoveProject()
    {
        var project = new Project { Id = Guid.NewGuid(), Name = "ToDelete", UserId = _userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        var handler = new DeleteProjectHandler(_dbContext);
        var result = await handler.Handle(new DeleteProjectCommand { ProjectId = project.Id, UserId = _userId }, CancellationToken.None);

        result.Should().BeTrue();
        var gone = await _dbContext.Projects.FindAsync(project.Id);
        gone.Should().BeNull();
    }

    [Fact]
    public async Task DeleteProject_ShouldThrow_WhenBelongsToDifferentUser()
    {
        var project = new Project { Id = Guid.NewGuid(), Name = "Someone else's", UserId = "other-user", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        var handler = new DeleteProjectHandler(_dbContext);
        await handler.Invoking(h => h.Handle(new DeleteProjectCommand { ProjectId = project.Id, UserId = _userId }, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }
}
