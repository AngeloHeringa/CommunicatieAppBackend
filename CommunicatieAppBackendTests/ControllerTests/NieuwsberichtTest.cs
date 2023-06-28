using CommunicatieAppBackend.Controllers;
using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Hubs;
using CommunicatieAppBackend.Models;
using CommunicatieAppBackendTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CommunicatieAppBackend.Tests.Controllers
{
    public class NieuwsberichtControllerTests
    {
        private readonly NieuwsberichtController _controller;
        private readonly Mock<AppDbContext> _contextMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;

        public NieuwsberichtControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            _contextMock = new Mock<AppDbContext>(options);
            var mockHub = new Mock<IHubContextTest>();
            var hubClientsMock = new Mock<IHubClients>();
            var hubClientProxyMock = new Mock<IClientProxy>();
            hubClientsMock.Setup(x=>x.All).Returns(hubClientProxyMock.Object);
            mockHub.Setup(x=>x.Clients).Returns(hubClientsMock.Object);
            mockHub.Setup(a => a.sendNotification(It.IsAny<string>(),It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _environmentMock = new Mock<IWebHostEnvironment>();
            _controller = new NieuwsberichtController(_contextMock.Object, _environmentMock.Object, mockHub.Object);
        }

        [Fact]
        public async Task Index_WithSearchString_ReturnsViewResultWithNieuwsberichten()
        {
            // Arrange
            var searchString = "Test";
            var nieuwsberichten = new List<Nieuwsbericht> { new Nieuwsbericht { Titel = "Test Nieuwsbericht" } }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.nieuwsberichten).Returns(nieuwsberichten.Object);

            // Act
            var result = await _controller.Index(searchString);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Nieuwsbericht>>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Count());
        }

        [Fact]
        public async Task Index_WithoutSearchString_ReturnsViewResultWithNieuwsberichten()
        {
            // Arrange
            var nieuwsberichten = new List<Nieuwsbericht> { new Nieuwsbericht { Titel = "Test Nieuwsbericht" } }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.nieuwsberichten).Returns(nieuwsberichten.Object);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Nieuwsbericht>>(viewResult.ViewData.Model);
            Assert.Equal(1, model.Count());
        }

        [Fact]
        public async Task Index_WithInvalidSearchString_ReturnsViewResultWithNoNieuwsberichten()
        {
            // Arrange
            var searchString = "test";
            var nieuwsberichten = new List<Nieuwsbericht>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.nieuwsberichten).Returns(nieuwsberichten.Object);

            // Act
            var result = await _controller.Index(searchString);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Nieuwsbericht>>(viewResult.ViewData.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewResultWithNieuwsbericht()
        {
            // Arrange
            var id = 1;
            var nieuwsbericht = new Nieuwsbericht { NieuwsberichtId = id };
            _contextMock.Setup(c => c.nieuwsberichten).Returns(new[] { nieuwsbericht }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.Details(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Nieuwsbericht>(viewResult.ViewData.Model);
            Assert.Equal(id, model.NieuwsberichtId);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var id = 1;
            _contextMock.Setup(c => c.nieuwsberichten).Returns(new List<Nieuwsbericht>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.Details(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_WithNullId_ReturnsNotFoundResult()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_WithoutModel_ReturnsViewResultWithNieuwsberichtViewModel()
        {
            // Arrange
            var locatie = new Locatie { Id = 1, name = "Location 1" };
            var locaties = new List<Locatie> { locatie }.AsQueryable().BuildMock();
            var locatiesMock = locaties.Object.BuildMockDbSet();
            locatiesMock.As<IAsyncEnumerable<Locatie>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Locatie>(locaties.Object.GetEnumerator()));
            locaties.As<IQueryable<Locatie>>().Setup(m => m.GetEnumerator()).Returns(locaties.Object.GetEnumerator());
            _contextMock.Setup(c => c.Locaties).Returns(locatiesMock.Object);

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<NieuwsberichtViewModel>(viewResult.Model);
            var locObj = locaties.Object;
            var modelLocs = model.Locaties;
            Assert.Equal(locatie, modelLocs.First());
        }

        [Fact]
        public async Task Create_WithValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var fileContents = "test;test;";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents));

            Mock<IFormFile> mock = new Mock<IFormFile>();
            mock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(stream);
                })
                .Returns(Task.CompletedTask);
            mock.Setup(x => x.Length).Returns(memoryStream.Length);
            mock.Setup(x => x.FileName).Returns("test.png");


            var model = new NieuwsberichtViewModel { Foto = mock.Object, nieuwsbericht = new Nieuwsbericht(){Image = mock.Object.FileName}};

            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");


            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(NieuwsberichtController.Index), redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Create_WithInvalidModel_ReturnsContentResult()
        {
            // Arrange
            var model = new NieuwsberichtViewModel { Foto = null };

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ContentResult>(result);
        }

        [Fact]
        public async Task Edit_WithValidId_ReturnsViewResultWithNieuwsberichtViewModel()
        {
            // Arrange
            var id = 1;
            var locatie = new Locatie { Id = id, name = "Location 1" };
            var locaties = new List<Locatie> { locatie }.AsQueryable().BuildMock();
            var locatiesMock = locaties.Object.BuildMockDbSet();
            locatiesMock.As<IAsyncEnumerable<Locatie>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Locatie>(locaties.Object.GetEnumerator()));
            locaties.As<IQueryable<Locatie>>().Setup(m => m.GetEnumerator()).Returns(locaties.Object.GetEnumerator());

            var fileContents = "test;test;";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContents));

            Mock<IFormFile> mock = new Mock<IFormFile>();
            mock.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) =>
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(stream);
                })
                .Returns(Task.CompletedTask);
            mock.Setup(x => x.Length).Returns(memoryStream.Length);
            mock.Setup(x => x.FileName).Returns("test.png");
            var nieuwsbericht = new Nieuwsbericht { NieuwsberichtId = id, Image = mock.Object.FileName };
            _contextMock.Setup(c => c.nieuwsberichten).Returns(new[] { nieuwsbericht }.AsQueryable().BuildMockDbSet().Object);
            _contextMock.Setup(c => c.Locaties).Returns(locatiesMock.Object);
            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            // Act
            var result = await _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsAssignableFrom<NieuwsberichtViewModel>(viewResult.ViewData.Model);
            Assert.Equal(nieuwsbericht, viewModel.nieuwsbericht);
            Assert.Equal(locatie, viewModel.Locaties.First());
            Assert.Equal(nieuwsbericht.Image, viewModel.Foto.FileName);
        }

        [Fact]
        public async Task Edit_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var id = 1;
            _contextMock.Setup(c => c.nieuwsberichten).Returns(new List<Nieuwsbericht>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithNullId_ReturnsNotFoundResult()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var nieuwsbericht = new Nieuwsbericht { NieuwsberichtId = 1, Titel=" ", Inhoud = " " , LocatieId=1};
            var nieuwsberichten = new List<Nieuwsbericht>(){nieuwsbericht}.AsQueryable().BuildMock();
            var nieuwsberichtenMockSet = nieuwsberichten.Object.BuildMockDbSet();
            
            var locatie = new Locatie { Id = 1, name="Test Locatie 1"};
            var locaties = new List<Locatie>(){locatie}.AsQueryable().BuildMock();
            var locatieMockSet = locaties.Object.BuildMockDbSet();

            var model = new NieuwsberichtViewModel { nieuwsbericht = nieuwsbericht };
            locatieMockSet.As<IAsyncEnumerable<Locatie>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Locatie>(locaties.Object.GetEnumerator()));

            locatieMockSet.As<IQueryable<Locatie>>().Setup(m => m.Expression).Returns(locaties.Object.Expression);
            locatieMockSet.As<IQueryable<Locatie>>().Setup(m => m.ElementType).Returns(locaties.Object.ElementType);
            locatieMockSet.As<IQueryable<Locatie>>().Setup(m => m.GetEnumerator()).Returns(() => locaties.Object.GetEnumerator());

            _contextMock.Setup(x=>x.Locaties).Returns(locatieMockSet.Object);

            // Act
            var result = await _controller.Edit(model.nieuwsbericht.NieuwsberichtId, model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(NieuwsberichtController.Index), redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_WithInvalidModel_ReturnsNotFoundResult()
        {
            // Arrange
            var nieuwsbericht = new Nieuwsbericht { NieuwsberichtId = 1, Titel=" ", Inhoud = " " , LocatieId=1};
            var nieuwsberichten = new List<Nieuwsbericht>(){nieuwsbericht}.AsQueryable().BuildMock();
            var nieuwsberichtenMockSet = nieuwsberichten.Object.BuildMockDbSet();
            
            var locatie = new Locatie { Id = 1, name="Test Locatie 1"};
            var locaties = new List<Locatie>(){locatie}.AsQueryable().BuildMock();
            var locatieMockSet = locaties.Object.BuildMockDbSet();

            var model = new NieuwsberichtViewModel { nieuwsbericht = nieuwsbericht };
            locatieMockSet.As<IAsyncEnumerable<Locatie>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Locatie>(locaties.Object.GetEnumerator()));

            locatieMockSet.As<IQueryable<Locatie>>().Setup(m => m.Expression).Returns(locaties.Object.Expression);
            locatieMockSet.As<IQueryable<Locatie>>().Setup(m => m.ElementType).Returns(locaties.Object.ElementType);
            locatieMockSet.As<IQueryable<Locatie>>().Setup(m => m.GetEnumerator()).Returns(() => locaties.Object.GetEnumerator());

            _contextMock.Setup(x=>x.Locaties).Returns(locatieMockSet.Object);

            _controller.ModelState.AddModelError("key", "error message");

            // Act
            var result = await _controller.Edit(model.nieuwsbericht.NieuwsberichtId+1, model); //invalid id

            // Assert
            var viewResult = Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsViewResultWithNieuwsbericht()
        {
            // Arrange
            int? id = 1;
            var nieuwsbericht = new Nieuwsbericht { NieuwsberichtId = (int)id };
            _contextMock.Setup(c => c.nieuwsberichten).Returns(new[] { nieuwsbericht }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Nieuwsbericht>(viewResult.ViewData.Model);
            Assert.Equal(id, model.NieuwsberichtId);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            int? id = 1;
            _contextMock.Setup(c => c.nieuwsberichten).Returns(new List<Nieuwsbericht>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithNullId_ReturnsNotFoundResult()
        {
            // Act
            var result = await _controller.Delete(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ReturnsRedirectToActionResult()
        {
            // Arrange
            var id = 1;
            var nieuwsbericht = new Nieuwsbericht { NieuwsberichtId = id };
            _contextMock.Setup(c => c.nieuwsberichten).Returns(new[] { nieuwsbericht }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(NieuwsberichtController.Index), redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var id = 1;
            _contextMock.Setup(c => c.nieuwsberichten).Returns(new List<Nieuwsbericht>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
