using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;
using StudentTrackerDAL.Infrastructure;
using StudentTrackerDAL.Repositories;
using StudentTrackerBLL.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<DashboardService>();


builder.Services.AddScoped<IAttendanceRepository>(provider =>
    new AttendanceRepository(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITestGradeRepository>(provider =>
    new TestGradeRepository(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ✅ Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
