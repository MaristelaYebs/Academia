using EduTrack.Data;
using EduTrack.Models;
using EduTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// Seed roles and admin
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Create schema first: use migrations when present, otherwise fallback to EnsureCreated.
    var hasMigrations = dbContext.Database.GetMigrations().Any();
    if (hasMigrations)
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    string[] roles = { "Admin", "Teacher", "Student" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Seed Admin
    var adminEmail = "admin@edutrack.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            FullName = "System Admin",
            UserName = adminEmail,
            Email = adminEmail,
            Role = "Admin"
        };
        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    var demoStudentNames = new[]
    {
        "John Mark Dela Cruz",
        "Maria Angelica Santos",
        "Joshua Miguel Reyes",
        "Anne Patricia Garcia",
        "Carlo Vincent Mendoza",
        "Kimberly Joy Flores",
        "Jericho Paolo Ramos",
        "Christine Mae Aquino",
        "Rafael Dominic Navarro",
        "Jasmine Claire Bautista",
        "Francis Ivan Castillo",
        "Nicole Andrea Villanueva",
        "Mark Anthony Salazar",
        "Danica Rose Cabrera",
        "Paolo Martin Gonzales",
        "Trisha Mae Fernandez",
        "Kevin Lloyd Morales",
        "Bea Therese Herrera",
        "Nathaniel Jude Ortega",
        "Alyssa Mae Valdez"
    };

    // Seed demo students (idempotent).
    for (int i = 1; i <= 20; i++)
    {
        var studentEmail = $"student{i:00}@edutrack.com";
        var existingStudent = await userManager.FindByEmailAsync(studentEmail);
        if (existingStudent != null)
        {
            if (existingStudent.FullName != demoStudentNames[i - 1])
            {
                existingStudent.FullName = demoStudentNames[i - 1];
                await userManager.UpdateAsync(existingStudent);
            }
            continue;
        }

        var student = new ApplicationUser
        {
            FullName = demoStudentNames[i - 1],
            UserName = studentEmail,
            Email = studentEmail,
            Role = "Student"
        };

        var createResult = await userManager.CreateAsync(student, "Student@123");
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(student, "Student");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();