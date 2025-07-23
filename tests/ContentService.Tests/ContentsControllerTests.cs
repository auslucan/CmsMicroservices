using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContentService.Controllers;
using ContentService.DTOs;
using ContentService.Models;
using ContentService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ContentService.Tests.Controllers
{
    public class ContentsControllerTests
    {
        private readonly Mock<IContentService> _mockService;
        private readonly ContentsController _controller;

        public ContentsControllerTests()
        {
            _mockService = new Mock<IContentService>();
            _controller = new ContentsController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithContents()
        {
            // Arrange
            var contents = new List<Content> { new() { Id = Guid.NewGuid(), Title = "Test" } };
            _mockService.Setup(s => s.GetAllContentsAsync()).ReturnsAsync(contents);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(contents);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_IfContentMissing()
        {
            _mockService.Setup(s => s.GetContentByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Content)null!);

            var result = await _controller.GetById(Guid.NewGuid());

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_IfContentExists()
        {
            var content = new Content { Id = Guid.NewGuid(), Title = "Test" };
            _mockService.Setup(s => s.GetContentByIdAsync(content.Id)).ReturnsAsync(content);

            var result = await _controller.GetById(content.Id);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(content);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedResult_WithLocationHeader()
        {
            var newContent = new Content { Id = Guid.NewGuid(), Title = "New" };
            _mockService.Setup(s => s.CreateContentAsync(It.IsAny<Content>())).ReturnsAsync(newContent);

            var result = await _controller.Create(newContent);

            var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            created.RouteValues!["id"].Should().Be(newContent.Id);
            created.Value.Should().BeEquivalentTo(newContent);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_IfContentMissing()
        {
            _mockService.Setup(s => s.UpdateContentAsync(It.IsAny<Guid>(), It.IsAny<Content>()))
                        .ReturnsAsync(false);

            var result = await _controller.Update(Guid.NewGuid(), new Content());

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Update_ShouldReturnNoContent_IfSuccess()
        {
            _mockService.Setup(s => s.UpdateContentAsync(It.IsAny<Guid>(), It.IsAny<Content>()))
                        .ReturnsAsync(true);

            var result = await _controller.Update(Guid.NewGuid(), new Content());

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_IfContentMissing()
        {
            _mockService.Setup(s => s.DeleteContentAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(false);

            var result = await _controller.Delete(Guid.NewGuid());

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Delete_ShouldReturnNoContent_IfSuccess()
        {
            _mockService.Setup(s => s.DeleteContentAsync(It.IsAny<Guid>()))
                        .ReturnsAsync(true);

            var result = await _controller.Delete(Guid.NewGuid());

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdateContentAndNotifyUser_ShouldReturnOk_IfSuccessful()
        {
            _mockService.Setup(s => s.UpdateContentAndUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<ContentUpdateDto>(),
                It.IsAny<Guid>()
            )).Returns(Task.CompletedTask);

            var dto = new ContentUpdateDto { Title = "Updated", Body = "test" };
            var result = await _controller.UpdateContentAndNotifyUser(Guid.NewGuid(), Guid.NewGuid(), dto);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(new { Message = "Content and user updated successfully." });
        }
        [Fact]
        public async Task UpdateContentAndNotifyUser_ShouldReturnServerError_OnException()
        {
            _mockService.Setup(s => s.UpdateContentAndUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<ContentUpdateDto>(),
                It.IsAny<Guid>()
            )).ThrowsAsync(new Exception("Something went wrong"));

            var dto = new ContentUpdateDto { Title = "Updated", Body = "test" };
            var result = await _controller.UpdateContentAndNotifyUser(Guid.NewGuid(), Guid.NewGuid(), dto);

            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(500);

            var value = objectResult.Value;
            value.Should().NotBeNull();

            var messageProp = value.GetType().GetProperty("Message");
            messageProp.Should().NotBeNull();
            var messageValue = messageProp.GetValue(value);
            messageValue.Should().Be("An error occurred while updating content and user.");
        }
    }
}