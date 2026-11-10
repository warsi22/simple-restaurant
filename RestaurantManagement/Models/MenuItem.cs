using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models;

// A dish or drink that the restaurant sells. Mapped to the "MenuItems" table.
public class MenuItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a name.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "1000", ErrorMessage = "Price must be between €0.01 and €1000.")]
    public decimal Price { get; set; }

    // Foreign key: every menu item belongs to one category.
    [Display(Name = "Category")]
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a category.")]
    public int CategoryId { get; set; }

    // Navigation property: lets us write menuItem.Category.Name.
    public Category? Category { get; set; }

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Created")]
    public DateTime CreatedDate { get; set; }
}
