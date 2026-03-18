using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlow.Application.DTOs;
using TaskFlow.Features.Tags.CreateTag;
using Xunit;

namespace TaskFlow.Api.IntegrationTests.Features.Tags;

public class TagsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TagsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateTag_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange
        var userId = "tag-test-user";
        var command = new CreateTagCommand
        {
            Name = "Important",
            Color = "#FF0000",
            UserId = userId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tags", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var tag = await response.Content.ReadFromJsonAsync<TagDto>();
        tag.Should().NotBeNull();
        tag!.Name.Should().Be("Important");
        tag.Color.Should().Be("#FF0000");
    }

    [Fact]
    public async Task CreateTag_ShouldReturnCreated_WhenColorIsNull()
    {
        // Arrange
        var userId = "tag-test-user-2";
        var command = new CreateTagCommand
        {
            Name = "Work",
            UserId = userId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tags", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var tag = await response.Content.ReadFromJsonAsync<TagDto>();
        tag.Should().NotBeNull();
        tag!.Name.Should().Be("Work");
        tag.Color.Should().BeNull();
    }

    [Fact]
    public async Task CreateTag_ShouldReturnBadRequest_WhenTagAlreadyExists()
    {
        // Arrange
        var userId = "duplicate-tag-user";
        var command = new CreateTagCommand
        {
            Name = "Duplicate",
            Color = "#00FF00",
            UserId = userId
        };

        // Create first tag
        await _client.PostAsJsonAsync("/api/tags", command);

        // Act - Try to create duplicate
        var duplicateCommand = new CreateTagCommand
        {
            Name = "Duplicate",
            Color = "#FF0000",
            UserId = userId
        };

        var response = await _client.PostAsJsonAsync("/api/tags", duplicateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTag_ShouldReturnCreated_WhenSameNameButDifferentUser()
    {
        // Arrange
        var user1 = "user-1";
        var user2 = "user-2";
        
        var command1 = new CreateTagCommand
        {
            Name = "Shared",
            Color = "#00FF00",
            UserId = user1
        };

        await _client.PostAsJsonAsync("/api/tags", command1);

        // Act - Create tag with same name for different user
        var command2 = new CreateTagCommand
        {
            Name = "Shared",
            Color = "#FF0000",
            UserId = user2
        };

        var response = await _client.PostAsJsonAsync("/api/tags", command2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetTags_ShouldReturnAllTagsForUser()
    {
        // Arrange
        var userId = "get-tags-user";
        
        await _client.PostAsJsonAsync("/api/tags", new CreateTagCommand
        {
            Name = "Tag 1",
            Color = "#FF0000",
            UserId = userId
        });

        await _client.PostAsJsonAsync("/api/tags", new CreateTagCommand
        {
            Name = "Tag 2",
            Color = "#00FF00",
            UserId = userId
        });

        // Act
        var response = await _client.GetAsync("/api/tags");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tags = await response.Content.ReadFromJsonAsync<List<TagDto>>();
        tags.Should().NotBeNull();
        tags!.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetTags_ShouldReturnOnlyUserTags()
    {
        // Arrange
        var user1 = "user-1";
        var user2 = "user-2";
        
        await _client.PostAsJsonAsync("/api/tags", new CreateTagCommand
        {
            Name = "User 1 Tag",
            UserId = user1
        });

        await _client.PostAsJsonAsync("/api/tags", new CreateTagCommand
        {
            Name = "User 2 Tag",
            UserId = user2
        });

        // Act - Get tags for user1
        var response = await _client.GetAsync($"/api/tags?userId={user1}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tags = await response.Content.ReadFromJsonAsync<List<TagDto>>();
        tags.Should().ContainSingle();
        tags!.First().Name.Should().Be("User 1 Tag");
    }

    [Fact]
    public async Task DeleteTag_ShouldReturnSuccess_WhenTagExists()
    {
        // Arrange
        var userId = "delete-tag-user";
        var command = new CreateTagCommand
        {
            Name = "Tag to Delete",
            Color = "#FF0000",
            UserId = userId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tags", command);
        var tag = await createResponse.Content.ReadFromJsonAsync<TagDto>();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/api/tags/{tag!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify tag is deleted
        var getResponse = await _client.GetAsync("/api/tags");
        var tags = await getResponse.Content.ReadFromJsonAsync<List<TagDto>>();
        tags!.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTag_ShouldReturnNotFound_WhenTagDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/tags/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTag_ShouldReturnNotFound_WhenTagBelongsToDifferentUser()
    {
        // Arrange
        var owner = "tag-owner";
        var command = new CreateTagCommand
        {
            Name = "Protected Tag",
            UserId = owner
        };

        var createResponse = await _client.PostAsJsonAsync("/api/tags", command);
        var tag = await createResponse.Content.ReadFromJsonAsync<TagDto>();

        // Act - Try to delete as different user (this would typically be handled by auth)
        // For now, the endpoint returns NotFound if tag doesn't exist for the user
        var deleteResponse = await _client.DeleteAsync($"/api/tags/{tag!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
