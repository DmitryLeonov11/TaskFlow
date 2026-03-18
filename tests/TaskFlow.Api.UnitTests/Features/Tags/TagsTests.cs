using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;
using TaskFlow.Features.Tags.CreateTag;
using TaskFlow.Features.Tags.DeleteTag;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.Api.UnitTests.Features.Tags;

public class CreateTagTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateTagHandler _handler;

    public CreateTagTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateTagHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldCreateTag_WhenNameIsUnique()
    {
        var userId = "user-123";
        var command = new CreateTagCommand { Name = "Important", Color = "#FF0000", UserId = userId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Important");
        result.Color.Should().Be("#FF0000");

        var tag = await _dbContext.Tags.FirstAsync();
        tag.Name.Should().Be("Important");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTagWithSameNameAlreadyExists()
    {
        var userId = "user-123";
        var existingTag = new Tag { Id = Guid.NewGuid(), Name = "Existing", Color = "#00FF00", UserId = userId, CreatedAt = DateTime.UtcNow };
        _dbContext.Tags.Add(existingTag);
        await _dbContext.SaveChangesAsync();

        var command = new CreateTagCommand { Name = "Existing", Color = "#FF0000", UserId = userId };
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Tag 'Existing' already exists");
    }

    [Fact]
    public async Task Handle_ShouldCreateTag_WhenSameNameButDifferentUser()
    {
        var user1 = "user-1";
        var user2 = "user-2";
        
        var existingTag = new Tag { Id = Guid.NewGuid(), Name = "Shared", Color = "#00FF00", UserId = user1, CreatedAt = DateTime.UtcNow };
        _dbContext.Tags.Add(existingTag);
        await _dbContext.SaveChangesAsync();

        var command = new CreateTagCommand { Name = "Shared", Color = "#FF0000", UserId = user2 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        var tags = await _dbContext.Tags.ToListAsync();
        tags.Should().HaveCount(2);
    }
}

public class DeleteTagTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteTagHandler _handler;

    public DeleteTagTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteTagHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldDeleteTag_WhenTagExists()
    {
        var userId = "user-123";
        var tagId = Guid.NewGuid();
        
        var tag = new Tag { Id = tagId, Name = "Tag to Delete", Color = "#FF0000", UserId = userId, CreatedAt = DateTime.UtcNow };
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteTagCommand { TagId = tagId, UserId = userId };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var deletedTag = await _dbContext.Tags.FindAsync(tagId);
        deletedTag.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTagNotFound()
    {
        var command = new DeleteTagCommand { TagId = Guid.NewGuid(), UserId = "user-123" };
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenTagBelongsToDifferentUser()
    {
        var tag = new Tag { Id = Guid.NewGuid(), Name = "Other User's Tag", Color = "#00FF00", UserId = "other-user", CreatedAt = DateTime.UtcNow };
        _dbContext.Tags.Add(tag);
        await _dbContext.SaveChangesAsync();

        var command = new DeleteTagCommand { TagId = tag.Id, UserId = "different-user" };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }
}
