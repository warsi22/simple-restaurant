using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models;

// One line of an order, e.g. "2 x Classic Burger".
public class OrderItem
{
    public int Id { get; set; }

    // Foreign key to the order this line belongs to.
    public int OrderId { get; set; }
    public Order? Order { get; set; }

    // Foreign key to the menu item that was ordered.
    public int MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }

    // The price is copied from the menu item at the time of ordering.
    // If the menu price changes later, old orders keep their original price.
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    // UnitPrice x Quantity
    public decimal Subtotal { get; set; }
}
