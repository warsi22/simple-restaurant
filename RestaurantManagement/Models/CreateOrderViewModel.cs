using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RestaurantManagement.Models;

// A ViewModel is a class made specifically for one View.
// The "New Order" form needs a customer name plus a quantity box for every menu item,
// which does not match any single database table, so we use this class instead.
public class CreateOrderViewModel
{
    [Required(ErrorMessage = "Please enter the customer's name.")]
    [StringLength(100)]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    public List<OrderLineViewModel> Lines { get; set; } = new();
}

// One row in the "New Order" form.
public class OrderLineViewModel
{
    public int MenuItemId { get; set; }

    // Display-only values. They are not posted back by the form, so we skip validating them.
    [ValidateNever]
    public string Name { get; set; } = string.Empty;

    [ValidateNever]
    public string CategoryName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    [Range(0, 50, ErrorMessage = "Quantity must be between 0 and 50.")]
    public int Quantity { get; set; }
}
