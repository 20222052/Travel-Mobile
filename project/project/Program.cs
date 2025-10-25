using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using project.Models;
using project.OtpConfig;
using project.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC + API
builder.Services.AddControllersWithViews();

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Email & các service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IOtpService, OtpService>();
builder.Services.AddTransient<IOrderEmailService, OrderEmailService>();

// CORS: Cho phép React Admin (Vite: 5173) và Mobile App
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin", cors =>
    {
        cors.WithOrigins(
                "http://localhost:5173",      // React Admin
                "http://10.0.2.2:5014",       // Android Emulator
                "http://localhost:5014"       // Local testing
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// AUTH
builder.Services.AddAuthentication(options =>
{
    // Giữ mặc định là UserScheme 
    options.DefaultScheme = "UserScheme";
    options.DefaultChallengeScheme = "UserScheme";
})
.AddCookie("UserScheme", options =>
{
    options.Cookie.Name = "UserCookie";
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Cho phép HTTP trong môi trường development (mobile app)
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401; // Unauthorized
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 403; // Forbidden
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
})
.AddCookie("AdminScheme", options =>
{
    options.Cookie.Name = "AdminCookie";
    
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Cho phép HTTP trong môi trường development (mobile app)
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    // Tránh redirect HTML đối với API
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
    options.AddPolicy("AdminOrEditor", p => p.RequireRole("ADMIN", "EDITOR"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS + static
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


