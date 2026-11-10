using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Models;
using RestaurantManagement.Repositories;

namespace RestaurantManagement.Controllers;

public class HomeController : Controller
{
    private readonly IMenuRepository _menuRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IOrderRepository _orderRepository;

    public HomeController(
        IMenuRepository menuRepository,
        ICategoryRepository categoryRepository,
        IOrderRepository orderRepository)
    {
        _menuRepository = menuRepository;
        _categoryRepository = categoryRepository;
        _orderRepository = orderRepository;
    }

    // GET: /  (the dashboard)
    public async Task<IActionResult> Index()
    {
        // Ask each repository for its numbers, then hand them to the View in one model.
        var dashboard = new DashboardViewModel
        {
            TotalMenuItems = await _menuRepository.CountAsync(),
            TotalCategories = await _categoryRepository.CountAsync(),
            TotalOrders = await _orderRepository.CountAsync(),
            TotalRevenue = await _orderRepository.GetTotalRevenueAsync(),
            RecentOrders = await _orderRepository.GetRecentAsync(5)
        };

        return View(dashboard);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
