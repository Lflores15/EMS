using EMS.Data;
using EMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotNetEnv; // For loading environment variables from .env

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from the .env file
Env.Load(); // This loads the .env file into the environment variables

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework and Identity services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configure Identity to use the custom User class
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout in minutes
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Set login route and other cookie settings
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Ensure the database is created and the default admin user exists
await EnsureAdminUser(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware for serving static files (e.g., images, CSS, JS)
app.UseStaticFiles();

// Enable routing
app.UseRouting();

// Use authentication and authorization
app.UseAuthentication(); // Adds cookie authentication
app.UseAuthorization();  // Adds authorization middleware

// Enable session middleware
app.UseSession();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Method to create a default admin user if none exists
async Task EnsureAdminUser(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Check if the "Admin" role exists, create it if not
    var roleExists = await roleManager.RoleExistsAsync("Admin");
    if (!roleExists)
    {
        var role = new IdentityRole("Admin");
        await roleManager.CreateAsync(role);
    }

    // Get admin email and password from environment variables
    var adminEmail = Env.GetString("DEFAULT_ADMIN_EMAIL"); // Load email from .env file
    var adminPassword = Env.GetString("ADMIN_PASSWORD"); // Load password from .env file

    // Check if the admin user exists, create one if not
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            // Assign the "Admin" role to the new admin user
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
