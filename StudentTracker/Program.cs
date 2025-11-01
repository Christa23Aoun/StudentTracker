using StudentTrackerBLL.Services;
using StudentTrackerBLL.Services.Dashboard;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerDAL.Infrastructure;
using StudentTrackerDAL.Repositories;

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
    client.BaseAddress = new Uri("https://localhost:7199/");
});

// ApiSettings binding
builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

// 🔹 Database connection factory
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

// 🔹 Repositories
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// 🔹 Manual string-based repositories
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<IAttendanceRepository>(_ => new AttendanceRepository(connectionString));
builder.Services.AddScoped<ITestGradeRepository>(_ => new TestGradeRepository(connectionString));

// 🔹 Services
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// ✅ build AFTER all service registrations
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
app.UseSession();
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
