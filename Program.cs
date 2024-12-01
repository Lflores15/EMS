using EMS.Data;
using EMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DotNetEnv; // For loading environment variables from .env

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from the .env file
Env.Load();

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure Entity Framework and Identity services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configure Identity to use the custom User class
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        // Optionally customize password and user requirements
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Set login route and other cookie settings
    options.LoginPath = "/Account/Login";  // Path to redirect when not authenticated
    options.LogoutPath = "/Account/Logout"; // Path to redirect after logout
    options.AccessDeniedPath = "/Account/AccessDenied"; // Path for access denied errors
});

var app = builder.Build();

// Ensure the database is created and the default admin user exists
await EnsureAdminUserAsync(app.Services);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();  // Ensure authentication happens before authorization
app.UseAuthorization();   // Ensure authorization happens after authentication
app.UseSession();         // Session management should be last in the order

// Default route setup
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

/// <summary>
/// Creates a default Admin role and Admin user if they don't exist.
/// </summary>
async Task EnsureAdminUserAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Role Name
    const string adminRole = "Admin";

    // Admin Credentials (from environment variables)
    var adminEmail = Env.GetString("DEFAULT_ADMIN_EMAIL");
    var adminPassword = Env.GetString("ADMIN_PASSWORD");

    if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
    {
        Console.WriteLine("Admin email or password not found in environment variables. Ensure the .env file contains 'DEFAULT_ADMIN_EMAIL' and 'ADMIN_PASSWORD'.");
        return;
    }

    // Create Admin Role if it doesn't exist
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        Console.WriteLine($"Creating role: {adminRole}");
        var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRole));
        if (!roleResult.Succeeded)
        {
            Console.WriteLine("Error creating Admin role:");
            foreach (var error in roleResult.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
            return;
        }
    }

    // Create Admin User if it doesn't exist
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        Console.WriteLine($"Creating admin user with email: {adminEmail}");
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // Mark email as confirmed
        };

        var userResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!userResult.Succeeded)
        {
            Console.WriteLine("Error creating Admin user:");
            foreach (var error in userResult.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
            return;
        }
    }

    // Ensure Admin User is in Admin Role
    if (!await userManager.IsInRoleAsync(adminUser, adminRole))
    {
        Console.WriteLine($"Adding user {adminEmail} to role {adminRole}");
        var roleAssignResult = await userManager.AddToRoleAsync(adminUser, adminRole);
        if (!roleAssignResult.Succeeded)
        {
            Console.WriteLine("Error assigning Admin role to user:");
            foreach (var error in roleAssignResult.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine($"User {adminEmail} is already in role {adminRole}");
    }
}
