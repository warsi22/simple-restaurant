using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Models;
using RestaurantManagement.Repositories;

namespace RestaurantManagement.Controllers;

// Handles every URL that starts with /Order
public class OrderController : Controller
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuRepository _menuRepository;

    public OrderController(IOrderRepository orderRepository, IMenuRepository menuRepository)
    {
        _orderRepository = orderRepository;
        _menuRepository = menuRepository;
    }

    // GET: /Order
    public async Task<IActionResult> Index()
    {
        var orders = await _orderRepository.GetAllAsync();
        return View(orders);
    }

    // GET: /Order/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    // GET: /Order/Create  -> a form listing every available menu item with a quantity box
    public async Task<IActionResult> Create()
    {
        var model = new CreateOrderViewModel
        {
            Lines = await BuildOrderLinesAsync()
        };

        return View(model);
    }

    // POST: /Order/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel model)
    {
        var selectedLines = model.Lines.Where(line => line.Quantity > 0).ToList();
        if (selectedLines.Count == 0)
        {
            // An empty key ("") shows the message in the validation summary at the top of the form.
            ModelState.AddModelError(string.Empty, "Please enter a quantity for at least one menu item.");
        }

        if (!ModelState.IsValid)
        {
            model.Lines = await BuildOrderLinesAsync(model.Lines);
            return View(model);
        }

        var order = new Order
        {
            CustomerName = model.CustomerName,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        foreach (var line in selectedLines)
        {
            // Always read the price from the database. Never trust a price sent by the browser.
            var menuItem = await _menuRepository.GetByIdAsync(line.MenuItemId);
            if (menuItem == null || !menuItem.IsAvailable)
            {
                continue;
            }

            // Copy today's price into the order item, so later price changes don't affect this order.
            order.OrderItems.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = line.Quantity,
                UnitPrice = menuItem.Price,
                Subtotal = menuItem.Price * line.Quantity
            });
        }

        if (order.OrderItems.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "None of the selected items are available any more.");
            model.Lines = await BuildOrderLinesAsync(model.Lines);
            return View(model);
        }

        // The total is calculated on the server from the stored subtotals.
        order.TotalAmount = order.OrderItems.Sum(item => item.Subtotal);

        await _orderRepository.AddAsync(order);

        TempData["SuccessMessage"] = $"Order #{order.Id} for {order.CustomerName} was created. Total: {order.TotalAmount:C}";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }

    // POST: /Order/UpdateStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        // Make sure the submitted value is one of Pending/Preparing/Completed/Cancelled.
        if (!Enum.IsDefined(status))
        {
            return BadRequest();
        }

        var updated = await _orderRepository.UpdateStatusAsync(id, status);
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = $"Order #{id} is now {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Builds one form row per available menu item.
    // If the form was already submitted, the quantities the user typed are kept.
    private async Task<List<OrderLineViewModel>> BuildOrderLinesAsync(List<OrderLineViewModel>? submittedLines = null)
    {
        var menuItems = await _menuRepository.GetAvailableAsync();

        return menuItems.Select(menuItem => new OrderLineViewModel
        {
            MenuItemId = menuItem.Id,
            Name = menuItem.Name,
            CategoryName = menuItem.Category?.Name ?? string.Empty,
            Price = menuItem.Price,
            Quantity = submittedLines?.FirstOrDefault(line => line.MenuItemId == menuItem.Id)?.Quantity ?? 0
        }).ToList();
    }
}
