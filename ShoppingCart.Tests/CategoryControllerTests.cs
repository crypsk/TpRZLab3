using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Tests.Datasets;
using ShoppingCart.Web.Areas.Admin.Controllers;
using System.Linq.Expressions;
using Xunit;

namespace ShoppingCart.Tests
{
    public class CategoryControllerTests
    {
        [Fact]
        public void GetCategories_All_ReturnAllCategories()
        {
            // Arrange
            Mock<ICategoryRepository> repositoryMock = new Mock<ICategoryRepository>();

            repositoryMock.Setup(r => r.GetAll(It.IsAny<string>()))
                .Returns(() => CategoryDataset.Categories);
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.Equal(CategoryDataset.Categories, result.Categories);
        }

        [Fact]
        public void GetCategory_ById_ReturnCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            var repositoryMock = new Mock<ICategoryRepository>();
            repositoryMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), null))
                .Returns(category);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            var result = controller.Get(categoryId);

            // Assert
            Assert.Equal(category, result.Category);
        }

        [Fact]
        public void CreateUpdateCategory_ValidModel_AddOrUpdateCategory()
        {
            // Arrange
            var category = new Category { Id = 0, Name = "New Category" };
            var categoryVM = new CategoryVM { Category = category };
            var repositoryMock = new Mock<ICategoryRepository>();

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            mockUnitOfWork.Setup(uow => uow.Save());

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.CreateUpdate(categoryVM);

            // Assert
            repositoryMock.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void CreateUpdateCategory_InvalidModel_ThrowsException()
        {
            // Arrange
            var controller = new CategoryController(null);
            controller.ModelState.AddModelError("Name", "Required");

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => controller.CreateUpdate(new CategoryVM()));
            Assert.Equal("Model is invalid", exception.Message);
        }

        [Fact]
        public void DeleteCategory_ValidId_DeletesCategory()
        {
            // Arrange
            int categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
                                
            var repositoryMock = new Mock<ICategoryRepository>();
            repositoryMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), null))
            .Returns(category);
                
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);
            mockUnitOfWork.Setup(uow => uow.Save());

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act
            controller.DeleteData(categoryId);

            // Assert
            repositoryMock.Verify(r => r.Delete(category), Times.Once);
            mockUnitOfWork.Verify(uow => uow.Save(), Times.Once);
        }

        [Fact]
        public void DeleteCategory_InvalidId_ThrowsException()
        {
            // Arrange
            var repositoryMock = new Mock<ICategoryRepository>();
            repositoryMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>(), null))
            .Returns((Category)null);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(uow => uow.Category).Returns(repositoryMock.Object);

            var controller = new CategoryController(mockUnitOfWork.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => controller.DeleteData(1));
            Assert.Equal("Category not found", exception.Message);
        }

    }
}