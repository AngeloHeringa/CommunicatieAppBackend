using CommunicatieAppBackend.Controllers;
using CommunicatieAppBackend.Hubs;
using CommunicatieAppBackend.Models;
using CommunicatieAppBackendTests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Build.ObjectModelRemoting;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace CommunicatieAppBackend.Tests.Controllers
{
    public class MeldingControllerTests
    {
        private Mock<AppDbContext> _contextMock;
        private MeldingController _controller;

        public MeldingControllerTests()
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

            _controller = new MeldingController(_contextMock.Object, mockHub.Object);
        }

        [Fact]
        public async Task Index_WithSearchString_ReturnsViewResultWithFilteredMeldingen()
        {
            // Arrange
            var searchString = "Test";
            var meldingen = new List<Melding>
            {
                new Melding { Titel = "Test 1", Inhoud = "Test content 1" },
                new Melding { Titel = "test 2", Inhoud = "Test content 2" }
            }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Index(searchString);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Melding>>(viewResult.Model);
            Assert.Equal(1, model.Count);
            Assert.Equal("Test 1", model[0].Titel);
        }

        [Fact]
        public async Task Index_WithoutSearchString_ReturnsViewResultWithAllMeldingen()
        {
            // Arrange
            var meldingen = new List<Melding>
            {
                new Melding { Titel = "Test 1", Inhoud = "Test content 1" },
                new Melding { Titel = "Test 2", Inhoud = "Test content 2" }
            }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Melding>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewResultWithMelding()
        {
            // Arrange
            var id = 1;
            var melding = new Melding { MeldingId = id, Titel = "Test Melding", Inhoud = "Test content" };
            var meldingen = new List<Melding> { melding }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Details(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Melding>(viewResult.Model);
            Assert.Equal(melding, model);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var id = 1;
            var meldingen = new List<Melding>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Details(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_WithoutModel_ReturnsViewResult()
        {
            // Arrange
            var locaties = new List<Locatie> { new Locatie { Id = 1, name = "Location 1" } }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.Locaties).Returns(locaties.Object);

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<MeldingViewModel>(viewResult.Model);
            Assert.Equal(1, model.Locaties.Count());
            Assert.Equal("Location 1", model.Locaties.First().name);
        }

        [Fact]
        public async Task Create_WithValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var melding = new Melding
                {
                    MeldingId = 1,
                    Titel = "Test Melding",
                    Inhoud = "Test content",
                    Datum = DateTime.UtcNow,
                    LocatieId = 1,
                    Locatie = new Locatie { Id = 1, name = "Location 1" },
                    Dringend = true
                };

            var model = new MeldingViewModel
            {
                melding = melding
            };
            var meldingenQueryable = new List<Melding> { melding }.AsQueryable().BuildMock();
            var meldingenMockSet = meldingenQueryable.Object.BuildMockDbSet();

            meldingenMockSet.As<IQueryable<Melding>>().Setup(m=>m.GetEnumerator()).Returns(meldingenQueryable.Object.GetEnumerator());

            // meldingenMockSet.Setup(s=>s.MaxAsync(
            //     It.IsAny<System.Linq.Expressions.Expression<Func<Melding, int>>>(),
            //     It.IsAny<CancellationToken>()
            // )).ReturnsAsync(melding.LocatieId);
            _contextMock.Setup(x=>x.meldingen).Returns(meldingenMockSet.Object);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_WithValidId_ReturnsViewResultWithMeldingViewModel()
        {
            // Arrange
            var locatie = new Locatie { Id = 1, name = "Location 1" };

            var id = 1;
            var melding = new Melding { MeldingId = id, Titel = "Test Melding", Inhoud = "Test content", Locatie = locatie, LocatieId = locatie.Id};
            var meldingenQueryable = new List<Melding> { melding }.AsQueryable().BuildMock();
            var meldingenMockSet = meldingenQueryable.Object.BuildMockDbSet();
            meldingenMockSet.Setup(s=>s.FindAsync(
                It.IsAny<CancellationToken>()
            )).ReturnsAsync(melding);
            meldingenMockSet.As<IQueryable<Melding>>().Setup(m=>m.GetEnumerator()).Returns(meldingenQueryable.Object.GetEnumerator());
            _contextMock.Setup(c => c.meldingen.FindAsync(It.IsAny<int?>()))
                .ReturnsAsync(melding);

            var locaties = new List<Locatie> { locatie }.AsQueryable().BuildMock();
            var locatiesMock = locaties.Object.BuildMockDbSet();
            locatiesMock.As<IAsyncEnumerable<Locatie>>().Setup(m => m.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<Locatie>(locaties.Object.GetEnumerator()));
            locaties.As<IQueryable<Locatie>>().Setup(m => m.GetEnumerator()).Returns(locaties.Object.GetEnumerator());
            _contextMock.Setup(c => c.Locaties).Returns(locatiesMock.Object);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<MeldingViewModel>(viewResult.Model);
            Assert.Equal(melding.Titel, model.melding.Titel);
            Assert.Equal(melding.Inhoud, model.melding.Inhoud);
            Assert.Equal(1, model.Locaties.Count());
            Assert.Equal("Location 1", model.Locaties.First().name);
        }

        [Fact]
        public async Task Edit_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var id = 1;
            var meldingen = new List<Melding>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Edit(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_WithValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new MeldingViewModel
            {
                melding = new Melding
                {
                    MeldingId = 1,
                    Titel = "Test Melding",
                    Inhoud = "Test content",
                    Datum = DateTime.UtcNow,
                    LocatieId = 1,
                    Locatie = new Locatie { Id = 1, name = "Location 1" },
                    Dringend = true
                }
            };
            var meldingen = new List<Melding> { model.melding }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Edit(model.melding.MeldingId, model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_WithInvalidModelId_ReturnsNotFoundResult()
        {
            // Arrange
            var model = new MeldingViewModel
            {
                melding = new Melding
                {
                    MeldingId = 1,
                    Titel = "Test Melding",
                    Inhoud = "Test content",
                    Datum = DateTime.UtcNow,
                    LocatieId = 1,
                    Locatie = new Locatie { Id = 1, name = "Location 1" },
                    Dringend = true
                }
            };
            var meldingen = new List<Melding>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Edit(2, model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsViewResultWithMelding()
        {
            // Arrange
            int? id = 1;
            var melding = new Melding { MeldingId = (int)id, Titel = "Test Melding", Inhoud = "Test content" };
            var meldingen = new List<Melding> { melding }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Melding>(viewResult.Model);
            Assert.Equal(melding, model);
        }

        [Fact]
        public async Task Delete_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var id = 1;
            var meldingen = new List<Melding>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_WithValidId_ReturnsRedirectToActionResult()
        {
            // Arrange
            var id = 1;
            var melding = new Melding { MeldingId = id, Titel = "Test Melding", Inhoud = "Test content" };
            var meldingen = new List<Melding> { melding }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var id = 1;
            var meldingen = new List<Melding>().AsQueryable().BuildMockDbSet();
            _contextMock.Setup(c => c.meldingen).Returns(meldingen.Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
