using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using RestaurantManagement.Data;
using RestaurantManagement.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------- 1. Register services (Dependency Injection container) ----------

// MVC: Controllers + Razor Views.
builder.Services.AddControllersWithViews();

// Razor normally writes characters like "€" as "&#x20AC;" in the HTML.
// Allowing all Unicode characters keeps the page source readable.
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

// The DbContext talks to PostgreSQL through the Npgsql provider.
// The connection string is read from appsettings.json ("ConnectionStrings:DefaultConnection"),
// so no database credentials are written in C# code.
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories. "Scoped" means one instance per HTTP request.
// When a controller asks for IMenuRepository in its constructor,
// ASP.NET Core automatically creates a MenuRepository and passes it in.
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

var app = builder.Build();

// Apply any pending EF Core migrations at startup, so a fresh database
// (e.g. on Railway) gets its tables created automatically.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
    db.Database.Migrate();
}

// ---------- 2. Configure the HTTP request pipeline ----------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serves files from wwwroot (CSS, JavaScript, Bootstrap, images).
app.UseStaticFiles();

// Irish English culture: prices are shown as "€9.50" and typed with a "." decimal point.
app.UseRequestLocalization("en-IE");

app.UseRouting();

// Routing: /Menu/Edit/5 -> MenuController.Edit(id: 5)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
