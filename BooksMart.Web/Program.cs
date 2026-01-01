using BooksMart.Data.Data;
using BooksMart.Data.DbInitializer;
using BooksMart.Data.Interfaces.Repository;
using BooksMart.Data.Interfaces.Repository.IRepository;
using BooksMart.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddAzureWebAppDiagnostics();

try
{
    Console.WriteLine("Starting application configuration...");
    // Add services to the container.
    builder.Services.AddControllersWithViews();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Connection String Retrieved: {!string.IsNullOrEmpty(connectionString)}");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
    }
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(connectionString));

    Console.WriteLine("DbContext configured");

    builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

    builder.Services.AddIdentity<IdentityUser, IdentityRole>().
        AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = $"/Identity/Account/Login";
        options.LogoutPath = $"/Identity/Account/Logout";
        options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

    });
    //builder.Services.AddDataProtection()
    //    .PersistKeysToFileSystem(new DirectoryInfo(@"./keys"))
    //    .SetApplicationName("BooksMart");

    builder.Services.AddDataProtection()
        .SetApplicationName("BooksMart");

    Console.WriteLine("Data protection configured");

    // Facebook authentication with validation
    var facebookAppId = builder.Configuration["Facebook:AppId"];
    var facebookAppSecret = builder.Configuration["Facebook:AppSecret"];

    if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
    {
        builder.Services.AddAuthentication().AddFacebook(options =>
        {
            options.AppId = facebookAppId;
            options.AppSecret = facebookAppSecret;
            options.CallbackPath = "/signin-facebook";
            options.SaveTokens = true;
        });
        Console.WriteLine("Facebook authentication configured");
    }
    else
    {
        Console.WriteLine("WARNING: Facebook authentication not configured");
    }

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(100);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    builder.Services.AddScoped<IDbInitializer, DbInitializer>();
    builder.Services.AddRazorPages();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IEmailSender, EmailSender>();

    Console.WriteLine("Services configured successfully");

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
    if (!string.IsNullOrEmpty(stripeSecretKey))
    {
        StripeConfiguration.ApiKey = stripeSecretKey;
        Console.WriteLine("Stripe configured");
    }
    else
    {
        Console.WriteLine("WARNING: Stripe not configured");
    }
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSession();

    Console.WriteLine("Middleware configured");

    try
    {
        SeedDatabase();
        Console.WriteLine("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR seeding database: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        // Don't throw - allow app to start even if seeding fails
    }

    app.MapRazorPages();
    app.MapControllerRoute(
        name: "default",
        pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

    Console.WriteLine("Application starting...");

    app.Run();


    void SeedDatabase()
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            dbInitializer.Initialize();
        }
    }

}
catch (Exception ex)
{
    Console.WriteLine($"FATAL ERROR during startup: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
    throw;
}