
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// MVC and Session configuration
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Add Authentication & Authorization (fixes your runtime error)
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
    });

builder.Services.AddAuthorization();

// HttpClient for API calls
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7199/");
});

// ApiSettings binding
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));


// ✅ Build AFTER all service registrations
var app = builder.Build();

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 🔹 Session + Auth
app.UseSession();
app.UseAuthentication(); // 👈 must come before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Model for API settings
public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}
