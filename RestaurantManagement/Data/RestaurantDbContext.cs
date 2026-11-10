using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Data;

// The DbContext is the bridge between our C# classes and the PostgreSQL database.
// Each DbSet becomes a table, and EF Core turns LINQ queries into SQL.
public class RestaurantDbContext : DbContext
{
    // The options (provider + connection string) are supplied by Program.cs.
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Extra database rules that Data Annotations cannot express.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Money columns: numeric(10,2) = up to 10 digits, 2 after the decimal point.
        modelBuilder.Entity<MenuItem>().Property(m => m.Price).HasPrecision(10, 2);
        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(10, 2);
        modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<OrderItem>().Property(oi => oi.Subtotal).HasPrecision(10, 2);

        // Store the status as readable text ("Pending") instead of a number (0).
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Category 1 ---- * MenuItem
        // Restrict = the database refuses to delete a category that still has menu items.
        modelBuilder.Entity<MenuItem>()
            .HasOne(m => m.Category)
            .WithMany(c => c.MenuItems)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order 1 ---- * OrderItem
        // Cascade = deleting an order also deletes its order items.
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // MenuItem 1 ---- * OrderItem
        // Restrict = a menu item that appears in past orders cannot be deleted.
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.MenuItem)
            .WithMany()
            .HasForeignKey(oi => oi.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);

        SeedData(modelBuilder);
    }

    // Sample data. EF Core puts these rows into the migration,
    // so they are inserted when you run "dotnet ef database update".
    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Burgers", Description = "Juicy grilled burgers served with fries" },
            new Category { Id = 2, Name = "Pizza", Description = "Stone-baked pizzas" },
            new Category { Id = 3, Name = "Drinks", Description = "Soft drinks and beverages" },
            new Category { Id = 4, Name = "Desserts", Description = "Sweet treats" });

        // Seed data must use fixed values, so we use a fixed UTC date instead of DateTime.UtcNow.
        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, Name = "Classic Burger", Description = "Beef patty, cheddar, lettuce, tomato", Price = 9.50m, CategoryId = 1, IsAvailable = true, CreatedDate = seedDate },
            new MenuItem { Id = 2, Name = "Chicken Burger", Description = "Crispy chicken fillet with mayo", Price = 10.50m, CategoryId = 1, IsAvailable = true, CreatedDate = seedDate },
            new MenuItem { Id = 3, Name = "Margherita Pizza", Description = "Tomato, mozzarella and basil", Price = 12.00m, CategoryId = 2, IsAvailable = true, CreatedDate = seedDate },
            new MenuItem { Id = 4, Name = "Chicken Pizza", Description = "Roast chicken, peppers and onion", Price = 14.00m, CategoryId = 2, IsAvailable = true, CreatedDate = seedDate },
            new MenuItem { Id = 5, Name = "Coca-Cola", Description = "330ml can", Price = 2.50m, CategoryId = 3, IsAvailable = true, CreatedDate = seedDate },
            new MenuItem { Id = 6, Name = "Chocolate Brownie", Description = "Warm brownie with vanilla ice cream", Price = 4.50m, CategoryId = 4, IsAvailable = true, CreatedDate = seedDate });
    }
}
