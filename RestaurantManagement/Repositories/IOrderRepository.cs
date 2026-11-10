using RestaurantManagement.Models;

namespace RestaurantManagement.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync();
    Task<List<Order>> GetRecentAsync(int count);
    Task<Order?> GetByIdAsync(int id);
    Task AddAsync(Order order);
    Task<bool> UpdateStatusAsync(int id, OrderStatus status);
    Task<int> CountAsync();
    Task<decimal> GetTotalRevenueAsync();
}
