using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Models;
using RestaurantManagement.Repositories;

namespace RestaurantManagement.Controllers;

// Handles every URL that starts with /Category
public class CategoryController : Controller
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryController(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    // GET: /Category
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return View(categories);
    }

    // GET: /Category/Create
    public IActionResult Create()
    {
        return View(new Category());
    }

    // POST: /Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        await _categoryRepository.AddAsync(category);

        TempData["SuccessMessage"] = $"Category \"{category.Name}\" was created.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Category/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: /Category/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
    {
        if (id != category.Id || !await _categoryRepository.ExistsAsync(id))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        await _categoryRepository.UpdateAsync(category);

        TempData["SuccessMessage"] = $"Category \"{category.Name}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Category/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: /Category/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        // Rule: a category that still contains menu items must not be deleted.
        if (await _categoryRepository.HasMenuItemsAsync(id))
        {
            TempData["ErrorMessage"] = $"Category \"{category.Name}\" cannot be deleted because it still contains menu items. " +
                                       "Move or delete those items first.";
            return RedirectToAction(nameof(Index));
        }

        await _categoryRepository.DeleteAsync(id);

        TempData["SuccessMessage"] = $"Category \"{category.Name}\" was deleted.";
        return RedirectToAction(nameof(Index));
    }
}
