using StudentTrackerDAL.Infrastructure;
using StudentTrackerDAL.Repositories;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerBLL.Services.Dashboard;

var builder = WebApplication.CreateBuilder(args);

// MVC
﻿var builder = WebApplication.CreateBuilder(args);

// MVC and Session configuration
builder.Services.AddControllersWithViews();
// ? Add HttpClient support for API communication
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7199/"); // ?? Your API base URL
});


// ---------- DAL wiring for MVC (same style you used in API) ----------
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

// Repos that use ISqlConnectionFactory
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();      // NOTE: Your UserRepository constructor must use ISqlConnectionFactory OR IConfiguration. If it expects IConfiguration, see the note below.

// Repos that were written with a string ctor (we saw TestGradeRepository/AttendanceRepository in your code)
builder.Services.AddScoped<ITestGradeRepository>(_ =>
    new TestGradeRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAttendanceRepository>(_ =>
    new AttendanceRepository(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- BLL services ----------
builder.Services.AddScoped<AdminDashboardService>();

var app = builder.Build();

// Pipeline
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
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Default route + your admin route

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Dashboard}",
    defaults: new { controller = "Admin" });

app.Run();
// ✅ This line starts the web server
app.Run();

// Model for API settings
public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}
