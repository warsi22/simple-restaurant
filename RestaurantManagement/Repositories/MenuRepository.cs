using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;

namespace RestaurantManagement.Repositories;

// The repository is the only place that reads and writes menu items in PostgreSQL.
public class MenuRepository : IMenuRepository
{
    private readonly RestaurantDbContext _context;

    // The DbContext is provided by dependency injection (see Program.cs).
    public MenuRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<List<MenuItem>> GetAllAsync()
    {
        // Include() also loads each item's Category, so the view can show the category name.
        return await _context.MenuItems
            .Include(m => m.Category)
            .OrderBy(m => m.Category!.Name)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<List<MenuItem>> GetAvailableAsync()
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.IsAvailable)
            .OrderBy(m => m.Category!.Name)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<MenuItem?> GetByIdAsync(int id)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(MenuItem menuItem)
    {
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();   // INSERT INTO "MenuItems" ...
    }

    public async Task UpdateAsync(MenuItem menuItem)
    {
        _context.MenuItems.Update(menuItem);

        // CreatedDate is not part of the edit form, so keep the value already in the database.
        _context.Entry(menuItem).Property(m => m.CreatedDate).IsModified = false;

        await _context.SaveChangesAsync();   // UPDATE "MenuItems" SET ...
    }

    public async Task DeleteAsync(int id)
    {
        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem != null)
        {
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();   // DELETE FROM "MenuItems" ...
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.MenuItems.AnyAsync(m => m.Id == id);
    }

    public async Task<bool> IsUsedInOrdersAsync(int id)
    {
        return await _context.OrderItems.AnyAsync(oi => oi.MenuItemId == id);
    }

    public async Task<int> CountAsync()
    {
        return await _context.MenuItems.CountAsync();
    }
}
