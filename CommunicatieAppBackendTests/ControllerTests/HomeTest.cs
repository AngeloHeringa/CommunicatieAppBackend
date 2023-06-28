using System.Security.Claims;
using System.Threading.Tasks;
using CommunicatieAppBackend.Controllers;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace CommunicatieAppBackend.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null);

            _controller = new HomeController(_loggerMock.Object, _userManagerMock.Object);
        }

        [Fact]
        public async Task IndexAsync_WithUser_ReturnsViewWithLoggedInData()
        {
            // Arrange
            var user = new IdentityUser { UserName = "testuser" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _userManagerMock.Setup(m => m.GetUserAsync(claimsPrincipal))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.IndexAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user.UserName, viewResult.ViewData["username"]);
            Assert.True((bool)viewResult.ViewData["isLoggedIn"]);
            Assert.Null(viewResult.ViewData["Requests"]); // Assuming no admin role for this test
        }

        [Fact]
        public async Task IndexAsync_WithoutUser_ReturnsViewWithoutLoggedInData()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = null }
            };

            // Act
            var result = await _controller.IndexAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewData["username"]);
            Assert.False((bool)viewResult.ViewData["isLoggedIn"]);
            Assert.Null(viewResult.ViewData["Requests"]);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}