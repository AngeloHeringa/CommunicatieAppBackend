using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicatieAppBackend;
using CommunicatieAppBackend.Controllers;
using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Models;
using CommunicatieAppBackendTests.ControllerTests;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Moq;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace CommunicatieAppBackendTests
{
    public class HandleidingControllerTests
    {
        private readonly HandleidingController _controller;
        private readonly Mock<AppDbContext> _contextMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;

        public HandleidingControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            _contextMock = new Mock<AppDbContext>(options);
            _environmentMock = new Mock<IWebHostEnvironment>();
            _controller = new HandleidingController(_contextMock.Object, _environmentMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            var handleidingen = new List<Handleiding>
            {
                new Handleiding { id = 1, Title = "Handleiding 1", Details = "Details 1", Document = "document1.pdf" },
                new Handleiding { id = 2, Title = "Handleiding 2", Details = "Details 2", Document = "document2.pdf" }
            };

            new Mock<DbSet<Handleiding>>().As<IQueryable<Handleiding>>().Setup(m => m.Provider).Returns(handleidingen.AsQueryable().Provider);
            new Mock<DbSet<Handleiding>>().As<IQueryable<Handleiding>>().Setup(m => m.Expression).Returns(handleidingen.AsQueryable().Expression);
            new Mock<DbSet<Handleiding>>().As<IQueryable<Handleiding>>().Setup(m => m.ElementType).Returns(handleidingen.AsQueryable().ElementType);
            new Mock<DbSet<Handleiding>>().As<IQueryable<Handleiding>>().Setup(m => m.GetEnumerator()).Returns(handleidingen.GetEnumerator());

            _contextMock.Setup(p => p.Handleidingen).Returns(new Mock<DbSet<Handleiding>>().Object);
            _contextMock.Setup(p => p.SaveChanges()).Returns(1);

            // Mocking the behavior of ToListAsync
            _contextMock.Setup(c => c.Handleidingen.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(handleidingen);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Handleiding>>(viewResult.ViewData.Model);
            var modelList = model.ToList();
            Assert.Equal(handleidingen, modelList);

        }

        [Fact]
        public async Task Create_Post_ReturnsRedirectToIndex()
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
            mock.Setup(x => x.FileName).Returns("test.csv");

            var model = new HandleidingViewModel
            {
                Handleiding = new Handleiding { Title = "Test Handleiding", Details = "Test Details", Document = "test.csv", id = 1 },
                Document = mock.Object
            };

            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
        
        [Fact]
        public async Task Edit_Post_WithValidId_ReturnsRedirectToIndex()
        {
            // Arrange
            var id = 1;
            var model = new HandleidingViewModel
            {
                Handleiding = new Handleiding { id = id, Title = "Test Handleiding", Details = "Test Details", Document = "test.pdf" },
                Document = new FormFile(Stream.Null, 0, 0, "testFile", "test.pdf")
            };
            _environmentMock.Setup(e => e.WebRootPath).Returns("wwwroot");
            _contextMock.Setup(c => c.Update(It.IsAny<Handleiding>())).Verifiable();
            _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask as Task<int>);

            // Act
            var result = await _controller.Edit(id, model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            _contextMock.Verify(c => c.Update(It.IsAny<Handleiding>()), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = 1;
            var model = new HandleidingViewModel { Handleiding = new Handleiding { id = 2 } };

            // Act
            var result = await _controller.Edit(id, model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Post_WithExistingId_ReturnsRedirectToIndex()
        {
            // Arrange
            var id = 1;
            var handleiding = new Handleiding { id = id, Title = "Test Handleiding", Details = "Test Details", Document = "test.pdf" };
            var handleidingen = new List<Handleiding> { handleiding };
            var handleidingenQueryable = handleidingen.AsQueryable();
            var handleidingenMockSet = new Mock<DbSet<Handleiding>>();
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.Provider).Returns(handleidingenQueryable.Provider);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.Expression).Returns(handleidingenQueryable.Expression);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.ElementType).Returns(handleidingenQueryable.ElementType);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.GetEnumerator()).Returns(handleidingenQueryable.GetEnumerator());
            _contextMock.Setup(c => c.Handleidingen).Returns(handleidingenMockSet.Object);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.DoesNotContain(handleiding, handleidingen);
        }

        [Fact]
        public async Task Delete_Post_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task getAll_ReturnsGetHandleidingDTO()
        {
            // Arrange
            var handleidingen = new List<Handleiding>
            {
                new Handleiding { id = 1, Title = "Handleiding 1", Details = "Details 1", Document = "document1.pdf" },
                new Handleiding { id = 2, Title = "Handleiding 2", Details = "Details 2", Document = "document2.pdf" }
            };
            var handleidingenQueryable = handleidingen.AsQueryable();
            var handleidingenMockSet = new Mock<DbSet<Handleiding>>();
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.Provider).Returns(handleidingenQueryable.Provider);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.Expression).Returns(handleidingenQueryable.Expression);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.ElementType).Returns(handleidingenQueryable.ElementType);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.GetEnumerator()).Returns(handleidingenQueryable.GetEnumerator());
            _contextMock.Setup(c => c.Handleidingen).Returns(handleidingenMockSet.Object);

            // Act
            var result = await _controller.getAll();

            // Assert
            var getHandleidingDTO = Assert.IsType<GetHandleidingDTO>(result);
            Assert.Equal(2, getHandleidingDTO.Handleidingen.Count);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsViewResult()
        {
            // Arrange
            var id = 1;
            var handleiding = new Handleiding { id = id, Title = "Test Handleiding", Details = "Test Details", Document = "test.pdf" };
            var handleidingen = new List<Handleiding> { handleiding };
            var handleidingenQueryable = handleidingen.AsQueryable();
            var handleidingenMockSet = new Mock<DbSet<Handleiding>>();
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.Provider).Returns(handleidingenQueryable.Provider);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.Expression).Returns(handleidingenQueryable.Expression);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.ElementType).Returns(handleidingenQueryable.ElementType);
            handleidingenMockSet.As<IQueryable<Handleiding>>().Setup(m => m.GetEnumerator()).Returns(handleidingenQueryable.GetEnumerator());
            _contextMock.Setup(c => c.Handleidingen).Returns(handleidingenMockSet.Object);

            // Act
            var result = await _controller.Details(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Handleiding>(viewResult.ViewData.Model);
            Assert.Equal(handleiding, model);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await _controller.Details(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

    }
    
}
  