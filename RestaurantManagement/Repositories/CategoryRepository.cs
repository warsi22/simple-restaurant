using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Data;
using RestaurantManagement.Models;

namespace RestaurantManagement.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly RestaurantDbContext _context;

    public CategoryRepository(RestaurantDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        // Include the menu items so the list page can show how many items each category has.
        return await _context.Categories
            .Include(c => c.MenuItems)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> HasMenuItemsAsync(int id)
    {
        return await _context.MenuItems.AnyAsync(m => m.CategoryId == id);
    }

    public async Task<int> CountAsync()
    {
        return await _context.Categories.CountAsync();
    }
}
