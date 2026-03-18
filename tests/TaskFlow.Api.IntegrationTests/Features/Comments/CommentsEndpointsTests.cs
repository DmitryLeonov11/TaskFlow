using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Application.DTOs;
using TaskFlow.Features.Comments.CreateComment;
using TaskFlow.Features.Tasks.CreateTask;
using Xunit;

namespace TaskFlow.Api.IntegrationTests.Features.Comments;

public class CommentsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CommentsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateComment_ShouldReturnCreated_WhenTaskExists()
    {
        // Arrange - Create a task first
        var userId = "integration-test-user";
        var taskCommand = new CreateTaskCommand
        {
            Title = "Task for Comment",
            Description = "This task will have comments",
            Priority = 2,
            UserId = userId
        };

        var taskResponse = await _client.PostAsJsonAsync("/api/tasks", taskCommand);
        taskResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var task = await taskResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        task.Should().NotBeNull();

        // Act - Create comment
        var commentCommand = new CreateCommentCommand
        {
            TaskId = task!.Id,
            UserId = userId,
            Content = "This is my first comment"
        };

        var commentResponse = await _client.PostAsJsonAsync($"/api/tasks/{task.Id}/comments", commentCommand);

        // Assert
        commentResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var comment = await commentResponse.Content.ReadFromJsonAsync<TaskCommentDto>();
        comment.Should().NotBeNull();
        comment!.Content.Should().Be("This is my first comment");
        comment.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var command = new CreateCommentCommand
        {
            TaskId = Guid.NewGuid(),
            UserId = "user-123",
            Content = "Comment for non-existent task"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/tasks/{command.TaskId}/comments", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnUnauthorized_WhenUserDoesNotOwnTask()
    {
        // Arrange - Create a task with different user
        var taskOwner = "task-owner";
        var taskCommand = new CreateTaskCommand
        {
            Title = "Protected Task",
            Description = "Only owner can comment",
            Priority = 1,
            UserId = taskOwner
        };

        var taskResponse = await _client.PostAsJsonAsync("/api/tasks", taskCommand);
        var task = await taskResponse.Content.ReadFromJsonAsync<TaskItemDto>();

        // Act - Try to comment as different user
        var commentCommand = new CreateCommentCommand
        {
            TaskId = task!.Id,
            UserId = "different-user",
            Content = "Unauthorized comment"
        };

        var response = await _client.PostAsJsonAsync($"/api/tasks/{task.Id}/comments", commentCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateComment_ShouldReturnBadRequest_WhenContentIsEmpty()
    {
        // Arrange - Create a task first
        var userId = "comment-test-user";
        var taskCommand = new CreateTaskCommand
        {
            Title = "Task for Empty Comment Test",
            Description = "Testing empty comment",
            Priority = 1,
            UserId = userId
        };

        var taskResponse = await _client.PostAsJsonAsync("/api/tasks", taskCommand);
        var task = await taskResponse.Content.ReadFromJsonAsync<TaskItemDto>();

        // Act - Create comment with empty content
        var commentCommand = new CreateCommentCommand
        {
            TaskId = task!.Id,
            UserId = userId,
            Content = ""
        };

        var response = await _client.PostAsJsonAsync($"/api/tasks/{task.Id}/comments", commentCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetComments_ShouldReturnCommentsForTask()
    {
        // Arrange - Create a task and add comments
        var userId = "get-comments-user";
        var taskCommand = new CreateTaskCommand
        {
            Title = "Task with Comments",
            Description = "This task will have multiple comments",
            Priority = 2,
            UserId = userId
        };

        var taskResponse = await _client.PostAsJsonAsync("/api/tasks", taskCommand);
        var task = await taskResponse.Content.ReadFromJsonAsync<TaskItemDto>();

        // Add comments
        var comment1 = new CreateCommentCommand
        {
            TaskId = task!.Id,
            UserId = userId,
            Content = "First comment"
        };
        var comment2 = new CreateCommentCommand
        {
            TaskId = task.Id,
            UserId = userId,
            Content = "Second comment"
        };

        await _client.PostAsJsonAsync($"/api/tasks/{task.Id}/comments", comment1);
        await _client.PostAsJsonAsync($"/api/tasks/{task.Id}/comments", comment2);

        // Act - Get comments
        var response = await _client.GetAsync($"/api/tasks/{task.Id}/comments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var comments = await response.Content.ReadFromJsonAsync<List<TaskCommentDto>>();
        comments.Should().NotBeNull();
        comments!.Count.Should().Be(2);
        comments.Select(c => c.Content).Should().Contain("First comment");
        comments.Select(c => c.Content).Should().Contain("Second comment");
    }

    [Fact]
    public async Task GetComments_ShouldReturnEmptyList_WhenTaskHasNoComments()
    {
        // Arrange - Create a task without comments
        var userId = "no-comments-user";
        var taskCommand = new CreateTaskCommand
        {
            Title = "Task without Comments",
            Description = "This task has no comments yet",
            Priority = 1,
            UserId = userId
        };

        var taskResponse = await _client.PostAsJsonAsync("/api/tasks", taskCommand);
        var task = await taskResponse.Content.ReadFromJsonAsync<TaskItemDto>();

        // Act - Get comments
        var response = await _client.GetAsync($"/api/tasks/{task!.Id}/comments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var comments = await response.Content.ReadFromJsonAsync<List<TaskCommentDto>>();
        comments.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnSuccess_WhenCommentExists()
    {
        // Arrange - Create a task and add a comment
        var userId = "delete-comment-user";
        var taskCommand = new CreateTaskCommand
        {
            Title = "Task for Delete Test",
            Description = "Testing comment deletion",
            Priority = 1,
            UserId = userId
        };

        var taskResponse = await _client.PostAsJsonAsync("/api/tasks", taskCommand);
        var task = await taskResponse.Content.ReadFromJsonAsync<TaskItemDto>();

        var commentCommand = new CreateCommentCommand
        {
            TaskId = task!.Id,
            UserId = userId,
            Content = "Comment to delete"
        };

        var commentResponse = await _client.PostAsJsonAsync($"/api/tasks/{task.Id}/comments", commentCommand);
        var comment = await commentResponse.Content.ReadFromJsonAsync<TaskCommentDto>();

        // Act - Delete comment
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{task.Id}/comments/{comment!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify comment is deleted
        var getResponse = await _client.GetAsync($"/api/tasks/{task.Id}/comments");
        var comments = await getResponse.Content.ReadFromJsonAsync<List<TaskCommentDto>>();
        comments!.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnNotFound_WhenCommentDoesNotExist()
    {
        // Arrange
        var commentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/tasks/some-task-id/comments/{commentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
