using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;

namespace RestaurantManagement.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly RestaurantDbContext _context;

    public OrderRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetRecentAsync(int count)
    {
        return await _context.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        // ThenInclude loads the MenuItem of every OrderItem, so we can show the item names.
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task AddAsync(Order order)
    {
        // Adding the order also adds all of its OrderItems in the same SaveChanges call.
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateStatusAsync(int id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return false;
        }

        order.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        // Cancelled orders do not earn money, so they are left out.
        return await _context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalAmount);
    }
}
