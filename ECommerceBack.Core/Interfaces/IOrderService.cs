using ECommerceBack.Core.Entities;
using ECommerceBack.Core.DTOs;

namespace ECommerceBack.Core.Interfaces
{
    public interface IOrderService
    {
        //Task<Order> CreateOrderFromCartAsync(int userId, PaymentInfo paymentInfo);
        Task<Order> CreateOrderDirectAsync(int userId, int productId, int quantity, string cardNumber);
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order?> GetOrderDetailsAsync(int orderId, int userId);
    }
}