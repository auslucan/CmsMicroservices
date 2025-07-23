using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;
using UserService.Services;
using FluentAssertions;

namespace UserService.Tests
{
    public class UserServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldAddUser()
        {
            var context = GetInMemoryDbContext();
            var service = new Services.UserService(context);
            var newUser = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User"
            };
            var createdUser = await service.CreateUserAsync(newUser);
            createdUser.Id.Should().NotBeEmpty();
            createdUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            var userInDb = await context.Users.FindAsync(createdUser.Id);
            userInDb.Should().NotBeNull();
            userInDb.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenExists()
        {
            var context = GetInMemoryDbContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "existinguser",
                Email = "exist@example.com",
                FullName = "Exist User",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new Services.UserService(context);

            var foundUser = await service.GetUserByIdAsync(user.Id);

            foundUser.Should().NotBeNull();
            foundUser.Username.Should().Be("existinguser");
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var context = GetInMemoryDbContext();
            var service = new Services.UserService(context);

            var user = await service.GetUserByIdAsync(Guid.NewGuid());

            user.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            var context = GetInMemoryDbContext();
            var service = new Services.UserService(context);

            var result = await service.UpdateUserAsync(Guid.NewGuid(), new User());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            var context = GetInMemoryDbContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "oldusername",
                Email = "old@example.com",
                FullName = "Old Name",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new Services.UserService(context);

            var updatedUser = new User
            {
                Username = "newusername",
                Email = "new@example.com",
                FullName = "New Name"
            };

            var result = await service.UpdateUserAsync(user.Id, updatedUser);

            result.Should().BeTrue();

            var userInDb = await context.Users.FindAsync(user.Id);
            userInDb.Username.Should().Be("newusername");
            userInDb.Email.Should().Be("new@example.com");
            userInDb.FullName.Should().Be("New Name");
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            var context = GetInMemoryDbContext();
            var service = new Services.UserService(context);

            var result = await service.DeleteUserAsync(Guid.NewGuid());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldRemoveUser_WhenUserExists()
        {
            var context = GetInMemoryDbContext();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "todelete",
                Email = "delete@example.com",
                FullName = "Delete User",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var service = new Services.UserService(context);

            var result = await service.DeleteUserAsync(user.Id);

            result.Should().BeTrue();
            var userInDb = await context.Users.FindAsync(user.Id);
            userInDb.Should().BeNull();
        }
    }
}