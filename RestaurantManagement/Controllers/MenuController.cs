using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantManagement.Models;
using RestaurantManagement.Repositories;

namespace RestaurantManagement.Controllers;

// Handles every URL that starts with /Menu
public class MenuController : Controller
{
    private readonly IMenuRepository _menuRepository;
    private readonly ICategoryRepository _categoryRepository;

    // Dependency Injection: ASP.NET Core creates the repositories and passes them in.
    // The controller never creates a DbContext itself.
    public MenuController(IMenuRepository menuRepository, ICategoryRepository categoryRepository)
    {
        _menuRepository = menuRepository;
        _categoryRepository = categoryRepository;
    }

    // GET: /Menu
    public async Task<IActionResult> Index()
    {
        // 1. Controller receives the HTTP request.
        // 2. Repository retrieves data from PostgreSQL.
        var menuItems = await _menuRepository.GetAllAsync();

        // 3. View displays the returned model (Views/Menu/Index.cshtml).
        return View(menuItems);
    }

    // GET: /Menu/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var menuItem = await _menuRepository.GetByIdAsync(id);
        if (menuItem == null)
        {
            return NotFound();
        }

        return View(menuItem);
    }

    // GET: /Menu/Create  -> shows the empty form
    public async Task<IActionResult> Create()
    {
        await LoadCategoryListAsync();
        return View(new MenuItem());
    }

    // POST: /Menu/Create  -> receives the submitted form
    // [Bind] lists the only fields we accept from the form (protects against "over-posting").
    // [ValidateAntiForgeryToken] checks the hidden token that the form tag helper adds.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,Price,CategoryId,IsAvailable")] MenuItem menuItem)
    {
        // Model binding has already copied the form fields into menuItem
        // and checked the Data Annotation rules.
        if (!ModelState.IsValid)
        {
            // Show the form again with the error messages.
            await LoadCategoryListAsync(menuItem.CategoryId);
            return View(menuItem);
        }

        // PostgreSQL "timestamp with time zone" columns expect UTC times.
        menuItem.CreatedDate = DateTime.UtcNow;
        await _menuRepository.AddAsync(menuItem);

        // TempData survives one redirect, so the next page can show the message.
        TempData["SuccessMessage"] = $"\"{menuItem.Name}\" was added to the menu.";

        // Post/Redirect/Get: redirecting stops the browser re-submitting the form on refresh.
        return RedirectToAction(nameof(Index));
    }

    // GET: /Menu/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var menuItem = await _menuRepository.GetByIdAsync(id);
        if (menuItem == null)
        {
            return NotFound();
        }

        await LoadCategoryListAsync(menuItem.CategoryId);
        return View(menuItem);
    }

    // POST: /Menu/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,CategoryId,IsAvailable")] MenuItem menuItem)
    {
        if (id != menuItem.Id || !await _menuRepository.ExistsAsync(id))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoryListAsync(menuItem.CategoryId);
            return View(menuItem);
        }

        await _menuRepository.UpdateAsync(menuItem);

        TempData["SuccessMessage"] = $"\"{menuItem.Name}\" was updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Menu/Delete/5  -> asks "Are you sure?"
    public async Task<IActionResult> Delete(int id)
    {
        var menuItem = await _menuRepository.GetByIdAsync(id);
        if (menuItem == null)
        {
            return NotFound();
        }

        return View(menuItem);
    }

    // POST: /Menu/Delete/5  -> actually deletes
    // ActionName("Delete") lets this method answer the same URL as the GET version above.
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var menuItem = await _menuRepository.GetByIdAsync(id);
        if (menuItem == null)
        {
            return NotFound();
        }

        // Past orders point to this menu item, so deleting it would break their history.
        if (await _menuRepository.IsUsedInOrdersAsync(id))
        {
            TempData["ErrorMessage"] = $"\"{menuItem.Name}\" cannot be deleted because it appears in existing orders. " +
                                       "Edit it and untick \"Available\" instead.";
            return RedirectToAction(nameof(Index));
        }

        await _menuRepository.DeleteAsync(id);

        TempData["SuccessMessage"] = $"\"{menuItem.Name}\" was deleted.";
        return RedirectToAction(nameof(Index));
    }

    // Fills the Category drop-down list used by the Create and Edit forms.
    private async Task LoadCategoryListAsync(int? selectedCategoryId = null)
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.CategoryList = new SelectList(categories, "Id", "Name", selectedCategoryId);
    }
}
