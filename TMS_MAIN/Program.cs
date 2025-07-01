
using Microsoft.EntityFrameworkCore;
using TMS_MAIN.Data;
using TMS_MAIN.Services;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies; // Added from first context
using Microsoft.Extensions.Logging; // Added for logger in catch block

internal class Program
{
    private static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Configure database context
        // Using the connection string from the second context which includes Configuration.GetConnectionString,
        // but preserving the explicit connection string from the first context as a fallback if DefaultConnection isn't found.
        builder.Services.AddDbContext<TreasuryManagementSystemContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection") ??
                "Server=LTIN522006\\SQLEXPRESS;Database=TMSMain;Integrated Security=true;TrustServerCertificate=True;Encrypt=False;", // Preferred connection string
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }
            ));

// Register your application services (combined from both files)
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<ITransactionService, CashFlowService>(); // Already present, but good to confirm
        IServiceCollection serviceCollection = builder.Services.AddScoped<IInvestmentService, InvestmentService>();
builder.Services.AddScoped<IViewRenderService, ViewRenderService>();
builder.Services.AddScoped<TMS_MAIN.Services.RiskManagementService>(); // Specific registration from first context
builder.Services.AddScoped<IRiskAssessmentService, RiskManagementService>(); // Specific registration from first context
builder.Services.AddSingleton<IConverter, SynchronizedConverter>(provider => new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<IReportService, ReportService>(); 
// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // From second context
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

        // Add HTTP Context Accessor (from both files)
        builder.Services.AddHttpContextAccessor();

        // Add authentication services (cookie-based) from the first context
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Keeping the 60 minutes from first context
            });

        // Add authorization services (from first context)
        builder.Services.AddAuthorization();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        // Session must be placed after UseRouting and before UseAuthentication/UseAuthorization
        app.UseSession();

        // Add authentication and authorization middleware (from first context)
        app.UseAuthentication();
        app.UseAuthorization();

        // Map controllers
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Account}/{action=Login}/{id?}"); // Default route from first context

        // Ensure database is created and seeded on application startup (from first context)
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<TreasuryManagementSystemContext>();
                context.Database.Migrate(); // Applies any pending migrations and creates the database if it doesn't exist
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred during database migration or seeding.");
            }
        }

        app.Run();
    }
}