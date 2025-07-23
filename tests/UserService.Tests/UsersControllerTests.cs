using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserService.Controllers;
using UserService.DTOs;
using UserService.Models;
using UserService.Services;
using Xunit;

namespace UserService.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockService = new Mock<IUserService>();
            _controller = new UsersController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfUsers()
        {
            var users = new List<User> { new User { Id = Guid.NewGuid(), Username = "ali" } };
            _mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(users, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenUserIsNull()
        {
            _mockService.Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User)null!);

            var result = await _controller.GetById(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenUserFound()
        {
            var user = new User { Id = Guid.NewGuid(), Username = "mehmet" };
            _mockService.Setup(s => s.GetUserByIdAsync(user.Id)).ReturnsAsync(user);

            var result = await _controller.GetById(user.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithUser()
        {
            var user = new User { Username = "deneme" };
            var createdUser = new User { Id = Guid.NewGuid(), Username = "deneme" };

            _mockService.Setup(s => s.CreateUserAsync(user)).ReturnsAsync(createdUser);

            var result = await _controller.Create(user);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(createdUser, createdAtResult.Value);
            Assert.Equal("GetById", createdAtResult.ActionName);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccess()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id };

            _mockService.Setup(s => s.UpdateUserAsync(id, user)).ReturnsAsync(true);

            var result = await _controller.Update(id, user);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var id = Guid.NewGuid();
            var user = new User();

            _mockService.Setup(s => s.UpdateUserAsync(id, user)).ReturnsAsync(false);

            var result = await _controller.Update(id, user);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccess()
        {
            var id = Guid.NewGuid();
            _mockService.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(true);

            var result = await _controller.Delete(id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenUserNotFound()
        {
            var id = Guid.NewGuid();
            _mockService.Setup(s => s.DeleteUserAsync(id)).ReturnsAsync(false);

            var result = await _controller.Delete(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateLastContentUpdated_ReturnsNoContent_WhenSuccessful()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id };
            var dto = new UpdateUserDto { LastContentUpdated = DateTime.UtcNow };

            _mockService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(user);
            _mockService.Setup(s => s.UpdateUserAsync(id, It.IsAny<User>())).ReturnsAsync(true);

            var result = await _controller.UpdateLastContentUpdated(id, dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateLastContentUpdated_ReturnsNotFound_WhenUserIsNull()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateUserDto { LastContentUpdated = DateTime.UtcNow };

            _mockService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync((User)null!);

            var result = await _controller.UpdateLastContentUpdated(id, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateLastContentUpdated_ReturnsServerError_WhenUpdateFails()
        {
            var id = Guid.NewGuid();
            var user = new User { Id = id };
            var dto = new UpdateUserDto { LastContentUpdated = DateTime.UtcNow };

            _mockService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(user);
            _mockService.Setup(s => s.UpdateUserAsync(id, It.IsAny<User>())).ReturnsAsync(false);

            var result = await _controller.UpdateLastContentUpdated(id, dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}