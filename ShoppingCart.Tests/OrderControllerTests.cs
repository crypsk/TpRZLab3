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
    public class OrderControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _controller = new OrderController(_unitOfWorkMock.Object);
        }

        [Fact]
        public void OrderDetails_ReturnsOrder_WhenOrderExists()
        {
            // Arrange
            var orderId = 1;
            var orderHeader = new OrderHeader { Id = orderId, ApplicationUser = new ApplicationUser() };
            var orderDetails = new List<OrderDetail> { new OrderDetail() };
            _unitOfWorkMock.Setup(u => u.OrderHeader.GetT(It.IsAny<System.Linq.Expressions.Expression<Func<OrderHeader, bool>>>(), "ApplicationUser"))
                           .Returns(orderHeader);
            _unitOfWorkMock.Setup(u => u.OrderDetail.GetAll(It.IsAny<string>()))
                           .Returns(orderDetails.AsQueryable());

            // Act
            var result = _controller.OrderDetails(orderId);

            // Assert
            result.Should().NotBeNull();
            result.OrderHeader.Should().BeEquivalentTo(orderHeader);
            result.OrderDetails.Should().BeEquivalentTo(orderDetails);
        }

        [Fact]
        public void SetToInProcess_UpdatesOrderStatus_WhenCalled()
        {
            // Arrange
            var orderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 1 } };
            _unitOfWorkMock.Setup(u => u.OrderHeader.UpdateStatus(It.IsAny<int>(), It.IsAny<string>()));

            // Act
            _controller.SetToInProcess(orderVM);

            // Assert
            _unitOfWorkMock.Verify(u => u.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, OrderStatus.StatusInProcess), Times.Once);
            _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void SetToShipped_UpdatesOrderStatus_WhenCalled()
        {
            // Arrange
            var orderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 1, Carrier = "Carrier", TrackingNumber = "12345" } };
            var orderHeader = new OrderHeader { Id = 1 };
            _unitOfWorkMock.Setup(u => u.OrderHeader.GetT(It.IsAny<System.Linq.Expressions.Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>()))
                           .Returns(orderHeader);
            _unitOfWorkMock.Setup(u => u.OrderHeader.Update(It.IsAny<OrderHeader>()));

            // Act
            _controller.SetToShipped(orderVM);

            // Assert
            _unitOfWorkMock.Verify(u => u.OrderHeader.Update(orderHeader), Times.Once);
            _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void SetToCancelOrder_UpdatesStatusAndRefund_WhenPaymentApproved()
        {
            // Arrange
            var orderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 1, PaymentStatus = PaymentStatus.StatusApproved, PaymentIntentId = "pi_123" } };
            var orderHeader = new OrderHeader { Id = 1, PaymentStatus = PaymentStatus.StatusApproved, PaymentIntentId = "pi_123" };
            _unitOfWorkMock.Setup(u => u.OrderHeader.GetT(It.IsAny<System.Linq.Expressions.Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>()))
                           .Returns(orderHeader);
            var refundServiceMock = new Mock<RefundService>();
            var refundOptions = new RefundCreateOptions { Reason = RefundReasons.RequestedByCustomer, PaymentIntent = orderHeader.PaymentIntentId };
            refundServiceMock.Setup(s => s.Create(It.IsAny<RefundCreateOptions>())).Returns(new Refund());
            
            // Act
            _controller.SetToCancelOrder(orderVM);

            // Assert
            refundServiceMock.Verify(s => s.Create(It.IsAny<RefundCreateOptions>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, OrderStatus.StatusCancelled), Times.Once);
            _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void SetToCancelOrder_UpdatesStatus_WhenPaymentNotApproved()
        {
            // Arrange
            var orderVM = new OrderVM { OrderHeader = new OrderHeader { Id = 1, PaymentStatus = PaymentStatus.StatusDeclined } };
            var orderHeader = new OrderHeader { Id = 1, PaymentStatus = PaymentStatus.StatusDeclined };
            _unitOfWorkMock.Setup(u => u.OrderHeader.GetT(It.IsAny<System.Linq.Expressions.Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>()))
                           .Returns(orderHeader);

            // Act
            _controller.SetToCancelOrder(orderVM);

            // Assert
            _unitOfWorkMock.Verify(u => u.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, OrderStatus.StatusCancelled), Times.Once);
            _unitOfWorkMock.Verify(u => u.Save(), Times.Once);
        }
    }
}