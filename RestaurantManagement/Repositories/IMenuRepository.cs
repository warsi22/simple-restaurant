using RestaurantManagement.Models;

namespace RestaurantManagement.Repositories;

// An interface describes WHAT the repository can do, not HOW.
// Controllers depend on this interface, so they never need to know about the DbContext.
public interface IMenuRepository
{
    Task<List<MenuItem>> GetAllAsync();
    Task<List<MenuItem>> GetAvailableAsync();
    Task<MenuItem?> GetByIdAsync(int id);
    Task AddAsync(MenuItem menuItem);
    Task UpdateAsync(MenuItem menuItem);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> IsUsedInOrdersAsync(int id);
    Task<int> CountAsync();
}
