using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models;

// The possible stages of an order. Stored in the database as text (e.g. "Pending").
public enum OrderStatus
{
    Pending,
    Preparing,
    Completed,
    Cancelled
}

// A customer's order. One order contains many OrderItems.
public class Order
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the customer's name.")]
    [StringLength(100)]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Display(Name = "Order Date")]
    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // The sum of all OrderItem subtotals, calculated when the order is created.
    [Display(Name = "Total")]
    public decimal TotalAmount { get; set; }

    // Navigation property: one order has many order items.
    public List<OrderItem> OrderItems { get; set; } = new();
}
