namespace RestaurantManagement.Models;

// The numbers shown on the Home page dashboard.
public class DashboardViewModel
{
    public int TotalMenuItems { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
}
