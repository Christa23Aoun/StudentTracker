using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ==============================================
// AUTHENTICATION — FIXED DEFAULT SCHEME
// ==============================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "SAPT.Auth";
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/Login";

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            var path = context.Request.Path.Value?.ToLower();

            if (path != null)
            {
                if (path.StartsWith("/admin"))
                    context.Response.Redirect("/Auth/LoginAdmin");
                else if (path.StartsWith("/teacher"))
                    context.Response.Redirect("/Auth/LoginTeacher");
                else if (path.StartsWith("/student"))
                    context.Response.Redirect("/Auth/LoginStudent");
                else
                    context.Response.Redirect("/Auth/Login");
            }

            return Task.CompletedTask;
        }
    };
});

// ==============================================
// SESSION
// ==============================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization();

// ==============================================
// API CLIENT
// ==============================================
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7199/api/");
});

// BIND API SETTINGS
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// **ORDER IS IMPORTANT**
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}
