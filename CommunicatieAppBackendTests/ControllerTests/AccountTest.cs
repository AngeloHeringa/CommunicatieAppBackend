using CommunicatieAppBackend.Controllers;
using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Xunit;
using Assert = Xunit.Assert;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.IdentityModel.Tokens;

[assembly: InternalsVisibleTo("TestProject")]
namespace CommunicatieAppBackend.Tests.Controllers
{ 
    public class AccountControllerTests
    {
    private AccountController CreateAccountController()
    {
        var loggerMock = new Mock<ILogger<AccountController>>();
        var userManagerMock = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null, null, null, null, null, null, null, null);


        var emailSenderMock = new Mock<IEmailSender>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();

        httpContextAccessorMock.Setup(x => x.HttpContext.User.Identity.Name)
            .Returns("test@example.com");
        claimsFactoryMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>()))
            .Returns(Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, "test@example.com")
            }))));



        userManagerMock.Setup(x => x.FindByNameAsync("test@example.com"))
            .Returns(Task.FromResult(new IdentityUser{ UserName = "Example", Email = "test@example.com", LockoutEnabled = false }) as Task<IdentityUser?>);
        userManagerMock.Setup(x => x.FindByEmailAsync("test@example.com"))
            .Returns(Task.FromResult(new IdentityUser{ UserName = "Example", Email = "test@example.com", LockoutEnabled = false, Id="validUserId", PasswordHash=WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("ValidPassword")) }) as Task<IdentityUser?>);
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).Returns((string param) =>
        {
            return param == "nonexisting@example.com" ? Task.FromResult<IdentityUser?>(null) : (Task.FromResult<IdentityUser?>(new IdentityUser { UserName = "Example", Email = "test@example.com", LockoutEnabled = false, Id = "validUserId", PasswordHash = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("ValidPassword")) }) as Task<IdentityUser?>);
        });
        userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(new IdentityUser{ UserName = "Example", Email = "test@example.com", LockoutEnabled = false}) as Task<IdentityUser?>);
        // userManagerMock.Setup(x => x.Users)
        //     .Returns( );
        var users = new[]{ new IdentityUser{UserName = "Example", Email = "test@example.com", LockoutEnabled = false} }.ToList();
        userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<IdentityUser, string>((x, y) => users.Add(x));
        userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<IdentityUser>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(x => x.ConfirmEmailAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(x => x.ResetPasswordAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(),It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<IdentityUser>())).Returns(Task.FromResult("dGVzdA=="));
        userManagerMock.Setup(x=>x.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>())).Returns(Task.FromResult("dGVzdA=="));
        userManagerMock.Setup(x=>x.GetRolesAsync(It.IsAny<IdentityUser>())).Returns(Task.FromResult(new List<string>(){"Admin"} as IList<string>));

        var usermgrobj = userManagerMock.Object;
        usermgrobj.UserValidators.Add(new UserValidator<IdentityUser>());
        usermgrobj.PasswordValidators.Add(new PasswordValidator<IdentityUser>());

        // var configurationMock = new Mock<Configuration>();
        var inMemorySettings = new Dictionary<string, string> {
            {"TopLevelKey", "TopLevelValue"},
            {"Jwt:Issuer", "SectionValue"},
            {"Jwt:Key", "SectionValueSectionValueSectionValueSectionValueSectionValueSectionValue"},
            {"Jwt:Audience", "SectionValue"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();


        var signInManagerMock = new Mock<SignInManager<IdentityUser>>(
            userManagerMock.Object, httpContextAccessorMock.Object, claimsFactoryMock.Object, null, null, null, null);

        signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<bool>()))
                         .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));
        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "test@example.com")
                }))
            }
        };
        var url = new Mock<IUrlHelper>();
        url.Setup(x=>x.Action(It.IsAny<UrlActionContext>())).Returns(
            "test.nl"
        );

        return new AccountController(
            loggerMock.Object,
            configuration,
            userManagerMock.Object,
            signInManagerMock.Object,
            emailSenderMock.Object)
        {
            ControllerContext = controllerContext,
            Url=url.Object
        };
    }
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var accountController = CreateAccountController();

            // Act
            var result = accountController.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Approve_WithValidId_ReturnsViewResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var id = "validId";

            // Act
            var result = await accountController.Approve(id);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task ApproveConfirm_WithValidId_ReturnsRedirectToActionResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var id = "validId";

            // Act
            var result = await accountController.ApproveConfirm(id);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Deny_WithValidId_ReturnsViewResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var id = "validId";

            // Act
            var result = await accountController.Deny(id);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DenyConfirm_WithValidId_ReturnsRedirectToActionResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var id = "validId";

            // Act
            var result = await accountController.DenyConfirm(id);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task ConfirmEmail_WithValidUserIdAndCode_ReturnsRedirectResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var userId = "validUserId";
            var code = "aG9paG9p";

            // Act
            var result = await accountController.ConfirmEmail(userId, code);

            // Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public async Task NewPassword_WithValidChangePasswordDTO_ReturnsTrue()
        {
            // Arrange
            var accountController = CreateAccountController();
            var dto = new ChangePasswordDTO
            {
                email = "test@example.com"
            };

            // Act
            var result = await accountController.NewPassword(dto);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task NewPasswordForm_WithValidUserIdAndCode_ReturnsViewResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var userId = "validUserId";
            var code = "validCode";

            // Act
            var result = await accountController.NewPasswordForm(userId, code);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task NewPasswordConfirm_WithValidViewModel_ReturnsRedirectResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var vm = new NewPasswordViewModel
            {
                userId = "validUserId",
                code = Base64UrlEncoder.Encode("validCode"),
                NewPassword = "newPassword"
            };

            // Act
            var result = await accountController.NewPasswordConfirm(vm);

            // Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public void Thankyou_ReturnsViewResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var status = "confirm";

            // Act
            var result = accountController.Thankyou(status);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task RegisterUser_WithValidRegisterDTO_ReturnsOkObjectResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var model = new RegisterDTO
            {
                Username = "testuser",
                Email = "nonexisting@example.com",
                Password = "password"
            };

            // Act
            var result = await accountController.RegisterUser(model);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithValidLoginDTO_ReturnsOkObjectResult()
        {
            // Arrange
            var accountController = CreateAccountController();
            var loginDto = new LoginDTO
            {
                Email = "test@example.com",
                Password = "password"
            };            

            // Act
            var result = await accountController.Login(loginDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUserData_ReturnsOkObjectResult()
        {
            // Arrange
            var accountController = CreateAccountController();

            // Act
            var result = await accountController.GetUserData();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
