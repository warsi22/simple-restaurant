using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Models;

// A Model represents application data.
// Entity Framework Core maps this class to the "Categories" table in PostgreSQL.
public class Category
{
    // "Id" is automatically used as the primary key.
    public int Id { get; set; }

    // Data Annotations are validation rules. They are checked when a form is submitted
    // and also shape the database column (e.g. NOT NULL, varchar(50)).
    [Required(ErrorMessage = "Please enter a category name.")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; set; }

    // Navigation property: one category has many menu items.
    public List<MenuItem> MenuItems { get; set; } = new();
}
