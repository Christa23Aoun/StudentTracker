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

// HttpClient for API calls
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7199/"); // your API URL
});

builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

var app = builder.Build();

// Error handling & middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ This line starts the web server
app.Run();

// Model for API settings
public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}
