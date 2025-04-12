using Moq;
using ShoppingCart.DataAccess.Repositories;
using ShoppingCart.DataAccess.ViewModels;
using ShoppingCart.Models;
using ShoppingCart.Web.Areas.Admin.Controllers;
using System;
using System.Linq.Expressions;
using Xunit;

namespace ShoppingCart.Tests
{
    public class CategoryControllerTests
    {
        [Fact]
        public void GetCategory_ById_ReturnsCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var categoryRepoMock = new Mock<ICategoryRepository>();
            categoryRepoMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>())).Returns(category);
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(uow => uow.Category).Returns(categoryRepoMock.Object);

            var controller = new CategoryController(unitOfWorkMock.Object);

            // Act
            var result = controller.Get(1);

            // Assert
            Assert.Equal(category, result.Category);
        }

        [Fact]
        public void CreateCategory_ValidModel_AddsCategoryAndSaves()
        {
            // Arrange
            var category = new Category { Id = 0, Name = "New Category" };
            var vm = new CategoryVM { Category = category };

            var categoryRepoMock = new Mock<ICategoryRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Category).Returns(categoryRepoMock.Object);

            var controller = new CategoryController(unitOfWorkMock.Object);
            controller.ModelState.Clear();

            // Act
            controller.CreateUpdate(vm);

            // Assert
            categoryRepoMock.Verify(r => r.Add(category), Times.Once);
            unitOfWorkMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void UpdateCategory_ValidModel_UpdatesCategoryAndSaves()
        {
            // Arrange
            var category = new Category { Id = 2, Name = "Updated Category" };
            var vm = new CategoryVM { Category = category };

            var categoryRepoMock = new Mock<ICategoryRepository>();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Category).Returns(categoryRepoMock.Object);

            var controller = new CategoryController(unitOfWorkMock.Object);
            controller.ModelState.Clear();

            // Act
            controller.CreateUpdate(vm);

            // Assert
            categoryRepoMock.Verify(r => r.Update(category), Times.Once);
            unitOfWorkMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void CreateUpdate_InvalidModel_ThrowsException()
        {
            // Arrange
            var vm = new CategoryVM { Category = new Category() };
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var controller = new CategoryController(unitOfWorkMock.Object);

            controller.ModelState.AddModelError("Name", "Required");

            // Act & Assert
            Assert.Throws<Exception>(() => controller.CreateUpdate(vm));
        }

        [Fact]
        public void DeleteCategory_ExistingId_DeletesCategoryAndSaves()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "To Be Deleted" };

            var categoryRepoMock = new Mock<ICategoryRepository>();
            categoryRepoMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>())).Returns(category);
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Category).Returns(categoryRepoMock.Object);

            var controller = new CategoryController(unitOfWorkMock.Object);

            // Act
            controller.DeleteData(1);

            // Assert
            categoryRepoMock.Verify(r => r.Delete(category), Times.Once);
            unitOfWorkMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void DeleteCategory_NonExistentId_ThrowsException()
        {
            // Arrange
            var categoryRepoMock = new Mock<ICategoryRepository>();
            categoryRepoMock.Setup(r => r.GetT(It.IsAny<Expression<Func<Category, bool>>>())).Returns((Category)null);
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.Setup(u => u.Category).Returns(categoryRepoMock.Object);

            var controller = new CategoryController(unitOfWorkMock.Object);

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => controller.DeleteData(999));
            Assert.Equal("Category not found", ex.Message);
        }
    }
}