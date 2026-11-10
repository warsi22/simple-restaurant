# The Simple Bistro – Restaurant Management System (ASP.NET Core MVC)

A small, beginner-friendly restaurant management application for learning **ASP.NET Core MVC**.
It uses **Entity Framework Core**, **PostgreSQL**, the **Repository Pattern**, **Razor Views** and **Bootstrap 5**.

Everything lives in **one** ASP.NET Core MVC project (`RestaurantManagement/`). There are no extra class libraries or layers.

Features:

- **Dashboard** – total menu items, categories, orders and revenue, plus the latest orders.
- **Menu** – list, view details, add, edit and delete menu items.
- **Categories** – list, add, edit and delete categories (a category that still has menu items cannot be deleted).
- **Orders** – create an order by entering quantities, see a live total, view all orders, view order details and change the status (Pending → Preparing → Completed / Cancelled).

---

## Table of contents

1. [Getting started](#1-getting-started)
2. [Project folder structure](#2-project-folder-structure)
3. [Database design](#3-database-design)
4. [Learning guide – MVC concepts](#4-learning-guide--mvc-concepts)
5. [Workflow 1 – Viewing the menu](#5-workflow-1--viewing-the-menu)
6. [Workflow 2 – Adding a menu item](#6-workflow-2--adding-a-menu-item)
7. [Ideas for extending the project](#7-ideas-for-extending-the-project)

---

## 1. Getting started

### Requirements

| Tool | Version used |
|---|---|
| .NET SDK | 9.0 (pinned by `global.json`) |
| PostgreSQL | 16 (any version from 13 works) |
| EF Core CLI (`dotnet-ef`) | 9.0.10, installed as a *local tool* (see `.config/dotnet-tools.json`) |

### Step 1 – Configure PostgreSQL

The connection string is in `RestaurantManagement/appsettings.json` and is called **`DefaultConnection`**:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=simple-restaurant;Username=careservice;Password=careservice_dev_only"
}
```

Change `Host`, `Port`, `Database`, `Username` and `Password` to match your own PostgreSQL server.
No credentials are written in C# code. `Program.cs` reads this value at startup.

The database user needs permission to create tables. Connect as a superuser (for example `postgres`)
using **psql** or **pgAdmin** and run:

```sql
-- Create the login (skip if it already exists)
CREATE ROLE careservice WITH LOGIN PASSWORD 'careservice_dev_only';

-- Create the database owned by that login (skip if it already exists)
CREATE DATABASE "simple-restaurant" OWNER careservice;

-- If the database already exists but belongs to another user, hand it over instead:
ALTER DATABASE "simple-restaurant" OWNER TO careservice;
```

> **Why is this needed?** Since PostgreSQL 15, ordinary users can no longer create tables in the `public`
> schema of a database they do not own. If you see `permission denied for schema public` when running
> `dotnet ef database update`, run the `ALTER DATABASE ... OWNER TO ...` command above.

On Windows, psql is usually at `C:\Program Files\PostgreSQL\16\bin\psql.exe`:

```powershell
& "C:\Program Files\PostgreSQL\16\bin\psql.exe" -h localhost -U postgres -c "ALTER DATABASE \"simple-restaurant\" OWNER TO careservice;"
```

### Step 2 – Restore tools and packages

From the repository root:

```bash
dotnet tool restore     # installs the local dotnet-ef tool
dotnet restore RestaurantManagement
```

### Step 3 – Create the database tables (migrations)

```bash
cd RestaurantManagement
dotnet ef database update
```

This creates the `Categories`, `MenuItems`, `Orders` and `OrderItems` tables and inserts the sample data.

The `InitialCreate` migration is **already included** in the `Migrations/` folder. To practise creating it yourself:

```bash
cd RestaurantManagement
dotnet ef database drop          # removes the database (answer "y")
rm -r Migrations                 # PowerShell: Remove-Item -Recurse Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Step 4 – Run the application

```bash
cd RestaurantManagement
dotnet run
```

Open the URL shown in the terminal (for example `http://localhost:5252`).
You can also press **F5** in Visual Studio or VS Code.

### Sample data

| Category | Menu items |
|---|---|
| Burgers | Classic Burger – €9.50, Chicken Burger – €10.50 |
| Pizza | Margherita Pizza – €12.00, Chicken Pizza – €14.00 |
| Drinks | Coca-Cola – €2.50 |
| Desserts | Chocolate Brownie – €4.50 |

The sample data is defined in `Data/RestaurantDbContext.cs` (method `SeedData`) using EF Core's `HasData`.

---

## 2. Project folder structure

```
simple-restaurant/
├── global.json                    ← pins the .NET SDK version
├── .config/dotnet-tools.json      ← local dotnet-ef tool
├── README.md                      ← this file
└── RestaurantManagement/          ← the ONE MVC project
    ├── Controllers/               ← C: receive requests, call repositories, choose a view
    │   ├── HomeController.cs          (dashboard)
    │   ├── MenuController.cs          (menu CRUD)
    │   ├── CategoryController.cs      (category CRUD)
    │   └── OrderController.cs         (create/view orders, change status)
    ├── Models/                    ← M: classes that describe our data
    │   ├── Category.cs
    │   ├── MenuItem.cs
    │   ├── Order.cs                   (also contains the OrderStatus enum)
    │   ├── OrderItem.cs
    │   ├── CreateOrderViewModel.cs    (data for the "New Order" form)
    │   ├── DashboardViewModel.cs      (data for the dashboard)
    │   └── ErrorViewModel.cs
    ├── Data/
    │   └── RestaurantDbContext.cs ← EF Core: tables, relationships, sample data
    ├── Repositories/              ← all database reading/writing happens here
    │   ├── IMenuRepository.cs / MenuRepository.cs
    │   ├── ICategoryRepository.cs / CategoryRepository.cs
    │   └── IOrderRepository.cs / OrderRepository.cs
    ├── Migrations/                ← generated by "dotnet ef migrations add"
    ├── Views/                     ← V: Razor (.cshtml) pages
    │   ├── Home/      Index
    │   ├── Menu/      Index, Details, Create, Edit, Delete
    │   ├── Category/  Index, Create, Edit, Delete
    │   ├── Order/     Index, Create, Details
    │   └── Shared/    _Layout (navbar), _Notifications (TempData alerts), _StatusBadge, Error
    ├── wwwroot/                   ← static files sent straight to the browser
    │   ├── css/site.css               (restaurant theme)
    │   ├── js/site.js
    │   ├── images/
    │   └── lib/                       (Bootstrap 5, jQuery, jQuery Validation)
    ├── appsettings.json           ← configuration, including the connection string
    ├── Program.cs                 ← app startup: services (DI) and request pipeline
    └── RestaurantManagement.csproj
```

**Naming conventions matter in MVC.** `MenuController` handles URLs that start with `/Menu`.
Its `Index()` action returns `View(...)`, which automatically uses `Views/Menu/Index.cshtml`.

---

## 3. Database design

```
Categories (1) ────< (many) MenuItems (1) ────< (many) OrderItems >──── (1) Orders
```

| Table | Columns | Notes |
|---|---|---|
| **Categories** | Id (PK), Name (varchar 50, required), Description (varchar 250) | |
| **MenuItems** | Id (PK), Name (varchar 100, required), Description (varchar 500), Price (numeric 10,2), CategoryId (FK), IsAvailable, CreatedDate (timestamptz) | FK → Categories, **Restrict** delete |
| **Orders** | Id (PK), CustomerName (varchar 100, required), OrderDate (timestamptz), Status (varchar 20), TotalAmount (numeric 10,2) | Status is stored as text: `Pending`, `Preparing`, `Completed`, `Cancelled` |
| **OrderItems** | Id (PK), OrderId (FK), MenuItemId (FK), Quantity, UnitPrice (numeric 10,2), Subtotal (numeric 10,2) | FK → Orders **Cascade**; FK → MenuItems **Restrict** |

Key design decisions:

- **`numeric(10,2)` for money.** Never use `float`/`double` for money because they have rounding errors.
- **`UnitPrice` is copied into `OrderItems`.** If the price of a Classic Burger changes from €9.50 to €10.00,
  old orders still show €9.50.
- **Restrict deletes.** The database refuses to delete a category that has menu items, or a menu item that
  appears in an order. The controllers check this first and show a friendly error message.
  (Tip: to remove an item that has been ordered before, edit it and untick **Available**.)

---

## 4. Learning guide – MVC concepts

### 4.1 What is ASP.NET Core MVC?

ASP.NET Core is Microsoft's framework for building web applications in C#.
**MVC** (Model–View–Controller) is a way of organising the code into three parts, each with one job:

| Part | Job | In this project |
|---|---|---|
| **Model** | The data | `MenuItem`, `Category`, `Order`, `OrderItem` |
| **View** | What the user sees (HTML) | `Views/Menu/Index.cshtml`, ... |
| **Controller** | Handles the request and connects the other two | `MenuController`, ... |

Keeping these separate makes the code easier to understand, test and change.
For example, you can redesign a page (View) without touching the database code.

### 4.2 What is a Model?

A model is a plain C# class that represents data. From `Models/MenuItem.cs`:

```csharp
public class MenuItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter a name.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000", ErrorMessage = "Price must be between €0.01 and €1000.")]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }          // foreign key
    public Category? Category { get; set; }      // navigation property
    ...
}
```

The `[Required]`, `[StringLength]` and `[Range]` attributes are **Data Annotations**. They are used for:

1. **Validation.** When a form is submitted, ASP.NET Core checks the rules and fills `ModelState`.
2. **Database shape.** EF Core turns `[Required]` into `NOT NULL` and `[StringLength(100)]` into `varchar(100)`.

A **ViewModel** is a model built for one particular page. `CreateOrderViewModel` holds the
customer name plus one quantity box per menu item, which doesn't match any single table.

### 4.3 What is a View?

A view is a `.cshtml` file: HTML mixed with C# using **Razor** syntax (`@`).
It only *displays* data; it doesn't load data or make decisions about it.
From `Views/Menu/Index.cshtml`:

```cshtml
@model List<MenuItem>

@foreach (var item in Model)
{
    <tr>
        <td>@item.Name</td>
        <td>@item.Category?.Name</td>
        <td>@item.Price.ToString("C")</td>
    </tr>
}
```

`@model List<MenuItem>` makes it a **strongly typed view**: Razor knows that `Model` is a list of
menu items, so you get IntelliSense and compile-time errors if you mistype a property name.

### 4.4 What is a Controller?

A controller is a class whose public methods (called **actions**) answer HTTP requests.
From `Controllers/MenuController.cs`:

```csharp
public class MenuController : Controller
{
    private readonly IMenuRepository _menuRepository;

    public MenuController(IMenuRepository menuRepository, ICategoryRepository categoryRepository)
    {
        _menuRepository = menuRepository;     // provided by dependency injection
        ...
    }

    // GET: /Menu
    public async Task<IActionResult> Index()
    {
        var menuItems = await _menuRepository.GetAllAsync();
        return View(menuItems);
    }
}
```

A controller should be "thin". It receives the request, asks a repository for data,
and picks which view to show. It does not contain SQL or HTML.

### 4.5 How does a request travel through an MVC application?

```
Browser  ──GET /Menu/Details/3──►  Routing  ──►  MenuController.Details(3)
                                                       │
                                                       ▼
                                              MenuRepository.GetByIdAsync(3)
                                                       │
                                                       ▼
                                              RestaurantDbContext ──SQL──► PostgreSQL
                                                       │
                                     MenuItem object ◄─┘
                                                       │
                                                       ▼
Browser  ◄────────── HTML ──────────  Views/Menu/Details.cshtml (Razor)
```

1. The browser requests `/Menu/Details/3`.
2. **Routing** (set up in `Program.cs` with the pattern `{controller=Home}/{action=Index}/{id?}`) maps it to
   controller = `Menu`, action = `Details`, id = `3`.
3. ASP.NET Core creates `MenuController`, **injecting** the repositories it needs.
4. The action calls the **repository**, which uses EF Core to query **PostgreSQL**.
5. The action passes the resulting **model** to `View(...)`.
6. **Razor** renders `Views/Menu/Details.cshtml` into HTML inside `_Layout.cshtml`, and the HTML is sent back.

### 4.6 What is Entity Framework Core?

EF Core is an **ORM** (Object–Relational Mapper). It lets you work with database rows as C# objects:

```csharp
// You write C#...
var items = await _context.MenuItems.Where(m => m.IsAvailable).ToListAsync();
```

```sql
-- ...EF Core generates and runs SQL like this for you:
SELECT * FROM "MenuItems" WHERE "IsAvailable";
```

`Include(m => m.Category)` tells EF Core to also load the related category (a SQL `JOIN`).

### 4.7 What is PostgreSQL?

PostgreSQL is a free, open-source **relational database**. It stores data in tables with rows and columns,
and it enforces rules such as primary keys, foreign keys and `NOT NULL`.
Our app talks to it through the **Npgsql** provider (`Npgsql.EntityFrameworkCore.PostgreSQL` NuGet package).

You can look at the data yourself with **pgAdmin** or **psql**:

```sql
SELECT "Name", "Price" FROM "MenuItems" ORDER BY "Price";
```

(PostgreSQL needs double quotes around names that contain capital letters.)

### 4.8 What is a DbContext?

The `DbContext` is EF Core's connection to the database for one unit of work.
`Data/RestaurantDbContext.cs` declares one `DbSet` per table:

```csharp
public DbSet<Category> Categories => Set<Category>();
public DbSet<MenuItem> MenuItems => Set<MenuItem>();
public DbSet<Order> Orders => Set<Order>();
public DbSet<OrderItem> OrderItems => Set<OrderItem>();
```

`OnModelCreating` adds rules that annotations cannot express: money precision `numeric(10,2)`,
relationships and delete behaviour, storing the order status as text, and the sample data.

It also **tracks changes**: after you call `Add`, `Update` or `Remove`, nothing happens in the database until
`SaveChangesAsync()` runs the SQL.

### 4.9 What is the Repository Pattern?

A **repository** is a class that hides all the database code for one kind of data behind simple methods.

```csharp
public interface IMenuRepository
{
    Task<List<MenuItem>> GetAllAsync();
    Task<MenuItem?> GetByIdAsync(int id);
    Task AddAsync(MenuItem menuItem);
    Task UpdateAsync(MenuItem menuItem);
    Task DeleteAsync(int id);
    ...
}
```

- `IMenuRepository` (the **interface**) says *what* can be done.
- `MenuRepository` (the **implementation**) says *how*, using `RestaurantDbContext`.

Benefits: controllers stay short and readable, database code lives in one place, and you could
swap the implementation (for example a fake in-memory one for testing) without changing controllers.

### 4.10 What is Dependency Injection?

Instead of a class creating the objects it needs (`new MenuRepository(...)`), it **asks for them in its
constructor**, and ASP.NET Core supplies them. The "recipe" is registered in `Program.cs`:

```csharp
builder.Services.AddDbContext<RestaurantDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
```

This reads as "whenever someone asks for an `IMenuRepository`, give them a `MenuRepository`."
**Scoped** means one instance per HTTP request. So for each request, ASP.NET Core:

1. creates a `RestaurantDbContext` with the connection string from `appsettings.json`,
2. creates a `MenuRepository`, passing in that DbContext,
3. creates a `MenuController`, passing in that repository.

### 4.11 What are migrations?

A **migration** is a C# file that describes how to change the database schema to match your models.

| Command | What it does |
|---|---|
| `dotnet ef migrations add InitialCreate` | Compares your models with the last migration and writes a new migration file in `Migrations/` |
| `dotnet ef database update` | Runs any migrations that haven't been applied yet against PostgreSQL |
| `dotnet ef migrations remove` | Deletes the most recent migration, if it hasn't been applied |
| `dotnet ef database drop` | Deletes the whole database |

Example: to add a `IsVegetarian` flag to menu items:

1. Add `public bool IsVegetarian { get; set; }` to `MenuItem.cs`.
2. Run `dotnet ef migrations add AddIsVegetarian`.
3. Run `dotnet ef database update`, which runs `ALTER TABLE "MenuItems" ADD "IsVegetarian" boolean ...`.

EF Core records applied migrations in the `__EFMigrationsHistory` table.

### 4.12 How do Razor Views work?

Razor switches between HTML and C# with the `@` symbol:

| Syntax | Meaning | Example |
|---|---|---|
| `@model Type` | The type of data this view receives | `@model MenuItem` |
| `@Model.Property` | Output a value (HTML-encoded, so it is safe) | `@Model.Name` |
| `@{ ... }` | A block of C# code | `@{ ViewData["Title"] = "Menu"; }` |
| `@if`, `@foreach`, `@for` | Control flow | `@foreach (var item in Model) { ... }` |
| `@* ... *@` | A Razor comment (not sent to the browser) | |

**Tag helpers** are HTML attributes starting with `asp-` that ASP.NET Core processes on the server:

```cshtml
<a asp-action="Edit" asp-route-id="@item.Id">Edit</a>      → <a href="/Menu/Edit/3">Edit</a>

<form asp-action="Create" method="post">                   → adds a hidden anti-forgery token
    <input asp-for="Name" class="form-control" />          → name="Name", value, validation attributes
    <span asp-validation-for="Name"></span>                → shows the validation message
</form>
```

**Layouts and partials.**
- `Views/Shared/_Layout.cshtml` holds the navbar and footer. Each page's content appears where `@RenderBody()` is.
- `Views/_ViewStart.cshtml` tells every view to use that layout.
- `<partial name="_Notifications" />` inserts a reusable piece of view, here the TempData success and error alerts.

---

## 5. Workflow 1 – Viewing the menu

**A student clicks "Menu" → MenuController receives the request → MenuRepository retrieves menu items →
PostgreSQL returns the records → the Controller passes the data to the View → Razor displays the menu.**

**Step 1 – The click.** In `_Layout.cshtml` the navbar link is:

```cshtml
<a class="nav-link" asp-controller="Menu" asp-action="Index">Menu</a>
```

The tag helper turns this into `<a href="/Menu">`. The browser sends **`GET /Menu`**.

**Step 2 – Routing → MenuController.** The route `{controller=Home}/{action=Index}/{id?}` maps `/Menu` to
`MenuController.Index()` (`Index` is the default action). ASP.NET Core builds the controller and injects
`IMenuRepository` (a `MenuRepository`, as registered in `Program.cs`).

```csharp
public async Task<IActionResult> Index()
{
    // 1. Controller receives the HTTP request.
    // 2. Repository retrieves data from PostgreSQL.
    var menuItems = await _menuRepository.GetAllAsync();

    // 3. View displays the returned model.
    return View(menuItems);
}
```

**Step 3 – MenuRepository queries the database.**

```csharp
public async Task<List<MenuItem>> GetAllAsync()
{
    return await _context.MenuItems
        .Include(m => m.Category)
        .OrderBy(m => m.Category!.Name)
        .ThenBy(m => m.Name)
        .ToListAsync();
}
```

**Step 4 – PostgreSQL returns the records.** EF Core translates the LINQ into SQL, roughly:

```sql
SELECT m.*, c.*
FROM "MenuItems" AS m
INNER JOIN "Categories" AS c ON m."CategoryId" = c."Id"
ORDER BY c."Name", m."Name";
```

PostgreSQL returns the rows, and EF Core turns each row into a `MenuItem` object with its `Category` filled in.
`await` frees the web server to handle other requests while it waits for the database.

**Step 5 – The Controller passes the data to the View.** `return View(menuItems);` uses the naming convention
to find `Views/Menu/Index.cshtml` and hands it the `List<MenuItem>` as its `Model`.

**Step 6 – Razor displays the menu.**

```cshtml
@model List<MenuItem>
...
@foreach (var item in Model)
{
    <tr>
        <td><strong>@item.Name</strong></td>
        <td>@item.Category?.Name</td>
        <td class="text-end">@item.Price.ToString("C")</td>   @* → €9.50 *@
        ...
    </tr>
}
```

Razor runs the loop on the server, produces plain HTML, places it in `_Layout.cshtml`, and the finished page
is sent to the browser.

---

## 6. Workflow 2 – Adding a menu item

**Step 1 – Show the empty form: `GET /Menu/Create`.**

```csharp
public async Task<IActionResult> Create()
{
    await LoadCategoryListAsync();          // fills ViewBag.CategoryList for the drop-down
    return View(new MenuItem());
}
```

`Views/Menu/Create.cshtml` renders the form:

```cshtml
<form asp-action="Create" method="post">
    <input asp-for="Name" class="form-control" />
    <span asp-validation-for="Name" class="text-danger"></span>
    <input asp-for="Price" class="form-control" type="number" step="0.01" />
    <select asp-for="CategoryId" asp-items="ViewBag.CategoryList" class="form-select">...</select>
    <input asp-for="IsAvailable" class="form-check-input" />
    <button type="submit">Save</button>
</form>
```

The form tag helper automatically adds a hidden `__RequestVerificationToken` field (**anti-forgery token**).
The `_ValidationScriptsPartial` scripts check the Data Annotation rules in the browser before submitting.

**Step 2 – The user submits: `POST /Menu/Create`.** The browser sends the form fields:

```
Name=Veggie Burger&Price=9.00&CategoryId=1&IsAvailable=true&__RequestVerificationToken=...
```

**Step 3 – The Controller receives the data.**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Name,Description,Price,CategoryId,IsAvailable")] MenuItem menuItem)
{
    if (!ModelState.IsValid)
    {
        await LoadCategoryListAsync(menuItem.CategoryId);
        return View(menuItem);                       // show the form again with error messages
    }

    menuItem.CreatedDate = DateTime.UtcNow;
    await _menuRepository.AddAsync(menuItem);

    TempData["SuccessMessage"] = $"\"{menuItem.Name}\" was added to the menu.";
    return RedirectToAction(nameof(Index));
}
```

- `[HttpPost]` means this action only answers form submissions.
- `[ValidateAntiForgeryToken]` rejects the request if the hidden token is missing or wrong. This stops other
  websites from submitting forms on the user's behalf (CSRF).
- **Model binding** creates a `MenuItem` and copies the form fields into it by name.
  `[Bind(...)]` lists the only fields allowed, so nobody can sneak in an `Id` or `CreatedDate`.
- **Validation:** ASP.NET Core checks `[Required]`, `[Range]` and the other annotations. If any rule fails,
  `ModelState.IsValid` is `false` and the same view is returned with error messages. The user's typing is kept.

**Step 4 – The Repository inserts the row.**

```csharp
public async Task AddAsync(MenuItem menuItem)
{
    _context.MenuItems.Add(menuItem);       // EF Core marks the object as "Added"
    await _context.SaveChangesAsync();      // runs the INSERT
}
```

```sql
INSERT INTO "MenuItems" ("CategoryId", "CreatedDate", "Description", "IsAvailable", "Name", "Price")
VALUES (1, '2026-11-02 10:15:00+00', NULL, TRUE, 'Veggie Burger', 9.00)
RETURNING "Id";
```

PostgreSQL generates the new `Id`, and EF Core writes it back into `menuItem.Id`.

**Step 5 – Redirect and show a message (Post/Redirect/Get).** `RedirectToAction(nameof(Index))` sends the
browser a *redirect* to `GET /Menu`. Refreshing the page will then reload the list instead of submitting
the form twice. `TempData` keeps the success message for exactly one more request, and
`_Notifications.cshtml` displays it as a green Bootstrap alert above the menu table.

---

## 7. Ideas for extending the project

- Add a search box or a category filter to the Menu page.
- Add a `TableNumber` field to orders (remember to add a migration!).
- Allow editing an order while it is still `Pending`.
- Add an image URL to menu items and show pictures from `wwwroot/images`.
- Show a "Best sellers" list on the dashboard using `OrderItems`.
- Add ASP.NET Core Identity for staff login (authentication was left out on purpose).
