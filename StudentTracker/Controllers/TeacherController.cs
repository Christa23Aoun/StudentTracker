using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class TeacherController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public TeacherController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var name = HttpContext.Session.GetString("UserName");
            var email = HttpContext.Session.GetString("UserEmail");

            if (userId == null)
                return RedirectToAction("LoginTeacher", "Auth");

            var res = await _client.GetAsync($"{_apiBase}Dashboard/Teacher/{userId}");

            if (!res.IsSuccessStatusCode)
                return View(new TeacherDashboardView());

            var json = await res.Content.ReadAsStringAsync();
            var vm = JsonConvert.DeserializeObject<TeacherDashboardView>(json) ?? new();

            vm.TeacherName = name ?? "";
            vm.TeacherEmail = email ?? "";

            return View("~/Views/Teacher/Dashboard.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetails(int id)
        {
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{id}");
            if (!courseRes.IsSuccessStatusCode)
                return NotFound();

            var courseJson = await courseRes.Content.ReadAsStringAsync();
            var course = JsonConvert.DeserializeObject<CourseDetailsView>(courseJson);

            var statsRes = await _client.GetAsync($"{_apiBase}Courses/{id}/stats");
            if (statsRes.IsSuccessStatusCode)
            {
                var statsJson = await statsRes.Content.ReadAsStringAsync();
                var stats = JsonConvert.DeserializeObject<CourseStatsView>(statsJson);

                course.StudentCount = stats.StudentCount;
                course.AverageGrade = stats.AverageGrade;
                course.AttendanceRate = stats.AttendanceRate;
            }

            return View("~/Views/Teacher/CourseDetails.cshtml", course);
        }

        [HttpGet]
        public async Task<IActionResult> StudentsInCourse(int courseId)
        {
            var res = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var students = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();

            ViewBag.CourseID = courseId;
            return View("~/Views/Teacher/StudentsInCourse.cshtml", students);
        }

        [HttpGet]
        public async Task<IActionResult> Schedule(int? semesterId, DateTime? weekStart)
        {
            var teacherId = HttpContext.Session.GetInt32("UserID");
            if (teacherId == null)
                return RedirectToAction("LoginTeacher", "Auth");

            var semestersRes = await _client.GetAsync($"{_apiBase}Semesters");
            var semestersJson = await semestersRes.Content.ReadAsStringAsync();
            var semesters = JsonConvert.DeserializeObject<List<SemesterView>>(semestersJson) ?? new();

            var selectedSemesterId = semesterId ?? semesters.FirstOrDefault()?.SemesterID ?? 0;

            var semesterOptions = semesters.Select(s => new SelectListItem
            {
                Value = s.SemesterID.ToString(),
                Text = s.Name,
                Selected = s.SemesterID == selectedSemesterId
            }).ToList();

            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var start = weekStart ?? today.AddDays(-diff);
            var end = start.AddDays(6);

            var endpoint =
                $"{_apiBase}Dashboard/Teacher/{teacherId}/schedule" +
                $"?semesterId={selectedSemesterId}" +
                $"&weekStart={start:yyyy-MM-dd}" +
                $"&weekEnd={end:yyyy-MM-dd}";

            var response = await _client.GetAsync(endpoint);

            var schedule = new List<TeacherScheduleItemView>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                schedule = JsonConvert.DeserializeObject<List<TeacherScheduleItemView>>(json) ?? new();
            }

            ViewBag.SemesterOptions = semesterOptions;
            ViewBag.SelectedSemesterId = selectedSemesterId;
            ViewBag.WeekStart = start;
            ViewBag.WeekEnd = end;

            return View("~/Views/Teacher/Schedule.cshtml", schedule);
        }
    }
}
