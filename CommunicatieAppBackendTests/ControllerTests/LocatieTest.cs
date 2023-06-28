using CommunicatieAppBackend.Controllers;
using CommunicatieAppBackend.DTOs;
using CommunicatieAppBackend.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Assert = Xunit.Assert;

namespace CommunicatieAppBackend.Tests.Controllers
{
    public interface IAppDbContext : IDisposable
    {
        IQueryable<Locatie> Locaties { get; }
        Task<int> SaveChangesAsync();
    }

    public class LocatieControllerTests
    {
        private LocatieController _locatieController;
        private MockAppDbContext _mockContext;

        public LocatieControllerTests()
        {
            // Example usage:
            var options = new DbContextOptionsBuilder<AppDbContext>().Options;
            _mockContext = new MockAppDbContext(options);

            // _mockContext = new Mock<AppDbContext>();
            _locatieController = new LocatieController(_mockContext);
        }

        [Fact]
        public async Task GetAll_ReturnsGetLocatiesDTO()
        {
            // Arrange
            var locaties = new List<Locatie>
            {
                new Locatie { Id = 11, name = "Location 1" },
                new Locatie { Id = 12, name = "Location 2" },
                new Locatie { Id = 13, name = "Location 3" }
            }.AsQueryable();

            //ensure empty:
            foreach (var entity in _mockContext.Locaties)
                _mockContext.Locaties.Remove(entity);
            _mockContext.SaveChanges();
            
            await _mockContext.Locaties.AddRangeAsync(locaties);

            await _mockContext.SaveChangesAsync(); // Save changes to the context

            _locatieController = new LocatieController(_mockContext);

            // Act
            var result = await _locatieController.getAll();

            // Assert
            Assert.IsType<GetLocatiesDTO>(result);
            Assert.Equal(locaties.Count(), result.locaties.Count);
        }
    }
}
