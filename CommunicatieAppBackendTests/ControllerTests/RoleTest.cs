using Xunit;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommunicatieAppBackend.Models;
using MockQueryable.Moq;

namespace CommunicatieAppBackend.Controllers.Tests
{
    public class RoleControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResultWithResult()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);

            var users = new List<IdentityUser>
            {
                new IdentityUser { Id = "1", UserName = "user1" },
                new IdentityUser { Id = "2", UserName = "user2" }
            }.AsQueryable().BuildMock();

            users.As<IAsyncEnumerable<IdentityUser>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<IdentityUser>(users.Object.GetEnumerator()));

            users.As<IQueryable<IdentityUser>>().Setup(m => m.Expression).Returns(users.Object.Expression);
            users.As<IQueryable<IdentityUser>>().Setup(m => m.ElementType).Returns(users.Object.ElementType);
            users.As<IQueryable<IdentityUser>>().Setup(m => m.GetEnumerator()).Returns(() => users.Object.GetEnumerator());


            userManagerMock.Setup(m => m.Users)
                .Returns(users.Object);

            userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(new List<string> { "Admin" });

            userManagerMock.Setup(m => m.IsInRoleAsync(It.IsAny<IdentityUser>(), "Admin"))
                .ReturnsAsync(true);

            roleManagerMock.Setup(m => m.RoleExistsAsync("Admin"))
                .ReturnsAsync(true);

            var controller = new RoleController(userManagerMock.Object, roleManagerMock.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<RoleViewModel>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task ToggleAdmin_RedirectsToIndex()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);

            var user = new IdentityUser { Id = "1" };


            userManagerMock.Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(user);

            userManagerMock.Setup(m => m.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(true);

            roleManagerMock.Setup(m => m.RoleExistsAsync("Admin"))
                .ReturnsAsync(true);

            var controller = new RoleController(userManagerMock.Object, roleManagerMock.Object);

            // Act
            var result = await controller.ToggleAdmin("1");

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
    }
}
