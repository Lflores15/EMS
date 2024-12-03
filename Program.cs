using EMS.Models;
using EMS.Data;
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
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure cookie-based authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";  // Redirect for unauthenticated access
    options.LogoutPath = "/Account/Logout"; // Redirect after logout
    options.AccessDeniedPath = "/Account/AccessDenied"; // Access denied page
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Set cookie expiration
    options.SlidingExpiration = true; // Ensure session is extended if active
});

// Build the application
var app = builder.Build();

// Ensure the roles and the database are created, and that the default admin user exists
await EnsureRolesAndAdminUserAsync(app.Services.CreateScope().ServiceProvider);

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
app.UseSession();         // Enable session support

// Default route setup
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();

// Ensure roles and a default Admin user are created at startup
async Task EnsureRolesAndAdminUserAsync(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Ensure roles exist
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Get admin email and password from environment variables
        var adminEmail = Env.GetString("DEFAULT_ADMIN_EMAIL");
        var adminPassword = Env.GetString("ADMIN_PASSWORD");

        // Check if the admin email and password are not set
        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin email or password not set in environment variables.");
        }

        // Check if the admin user already exists
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // Create the admin user with IsApproved set to true
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                IsApproved = true, // Admin is approved by default
                EmailConfirmed = true // Confirm the email
            };

            var userResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!userResult.Succeeded)
            {
                throw new Exception($"Error creating Admin user: {string.Join(", ", userResult.Errors.Select(e => e.Description))}");
            }

            // Assign the Admin role
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            // If the user exists, make sure IsApproved is true
            if (!adminUser.IsApproved)
            {
                adminUser.IsApproved = true;
                var updateResult = await userManager.UpdateAsync(adminUser);
                if (!updateResult.Succeeded)
                {
                    throw new Exception($"Error updating Admin user approval: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}

