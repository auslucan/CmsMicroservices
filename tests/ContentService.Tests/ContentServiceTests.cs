using ContentService.Data;
using ContentService.Models;
using ContentService.Services;
using ContentService.ClientServices;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ContentService.Tests.Services
{
    public class ContentServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllContentsAsync_ShouldReturnAllContents()
        {
            var context = GetDbContext();
            context.Contents.AddRange(
                new Content { Title = "test1", Body = "Body test1" },
                new Content { Title = "test2", Body = "Body test2" }
            );
            await context.SaveChangesAsync();

            var userClientMock = new Mock<IUserClientService>();
            var service = new ContentService.Services.ContentService(context, userClientMock.Object);

            var result = await service.GetAllContentsAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetContentByIdAsync_ShouldReturnCorrectContent()
        {
            var context = GetDbContext();
            var content = new Content { Id = Guid.NewGuid(), Title = "Test", Body = "Body" };
            context.Contents.Add(content);
            await context.SaveChangesAsync();

            var service = new ContentService.Services.ContentService(context, new Mock<IUserClientService>().Object);

            var result = await service.GetContentByIdAsync(content.Id);

            Assert.NotNull(result);
            Assert.Equal("Test", result!.Title);
        }

        [Fact]
        public async Task CreateContentAsync_ShouldAddNewContent()
        {
            var context = GetDbContext();
            var service = new ContentService.Services.ContentService(context, new Mock<IUserClientService>().Object);
            var content = new Content { Title = "New", Body = "Body" };

            var result = await service.CreateContentAsync(content);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("New", result.Title);
        }

        [Fact]
        public async Task UpdateContentAsync_ShouldUpdateIfExists()
        {
            var context = GetDbContext();
            var content = new Content { Id = Guid.NewGuid(), Title = "Old", Body = "Old Body" };
            context.Contents.Add(content);
            await context.SaveChangesAsync();

            var updated = new Content { Title = "New", Body = "Updated Body" };
            var service = new ContentService.Services.ContentService(context, new Mock<IUserClientService>().Object);

            var success = await service.UpdateContentAsync(content.Id, updated);

            Assert.True(success);
            var dbItem = await context.Contents.FindAsync(content.Id);
            Assert.Equal("New", dbItem!.Title);
        }

        [Fact]
        public async Task DeleteContentAsync_ShouldRemoveIfExists()
        {
            var context = GetDbContext();
            var content = new Content { Id = Guid.NewGuid(), Title = "To Delete", Body = "Delete body" };
            context.Contents.Add(content);
            await context.SaveChangesAsync();

            var service = new ContentService.Services.ContentService(context, new Mock<IUserClientService>().Object);

            var result = await service.DeleteContentAsync(content.Id);

            Assert.True(result);
            Assert.Empty(context.Contents);
        }

        [Fact]
        public async Task UpdateContentAndUserAsync_ShouldUpdateContentAndCallUserClient()
        {
            var context = GetDbContext();
            var content = new Content { Id = Guid.NewGuid(), Title = "Old", Body = "Body" };
            context.Contents.Add(content);
            await context.SaveChangesAsync();

            var userClientMock = new Mock<IUserClientService>();
            userClientMock.Setup(x => x.UpdateUserLastContentAsync(It.IsAny<Guid>(), It.IsAny<DTOs.UserUpdateDto>()))
                          .Returns(Task.CompletedTask);

            var service = new ContentService.Services.ContentService(context, userClientMock.Object);

            var dto = new DTOs.ContentUpdateDto { Title = "New", Body = "Updated Body" };

            await service.UpdateContentAndUserAsync(content.Id, dto, Guid.NewGuid());

            var updated = await context.Contents.FindAsync(content.Id);
            Assert.Equal("New", updated!.Title);
            userClientMock.Verify(x => x.UpdateUserLastContentAsync(It.IsAny<Guid>(), It.IsAny<DTOs.UserUpdateDto>()), Times.Once);
        }
    }
}