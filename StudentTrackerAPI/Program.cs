using Microsoft.OpenApi.Models;
using StudentTrackerCOMMON.Interfaces.Repositories;
using StudentTrackerCOMMON.Interfaces.Services;

using StudentTrackerDAL.Infrastructure;
using StudentTrackerDAL.Repositories;

using StudentTrackerBLL.Services;
using StudentTrackerBLL.Services.Dashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student Tracker API",
        Version = "v1"
    });
});

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<ICourseScheduleRepository>(provider =>
    new CourseScheduleRepository(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITestGradeRepository>(sp =>
    new TestGradeRepository(connectionString));
builder.Services.AddScoped<IAttendanceRepository>(sp =>
    new AttendanceRepository(connectionString));
builder.Services.AddScoped<IStudentDashboardRepository, StudentDashboardRepository>();

builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<ICourseScheduleService, CourseScheduleService>();
builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
builder.Services.AddScoped<IStudentCourseDetailsRepository, StudentCourseDetailsRepository>();
builder.Services.AddScoped<IStudentCourseDetailsService, StudentCourseDetailsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
