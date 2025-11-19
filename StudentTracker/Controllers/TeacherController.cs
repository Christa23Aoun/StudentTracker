using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class TeacherController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public TeacherController(IHttpClientFactory factory, IConfiguration config)
        {
            // FIXED: use the correct API client
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Dashboard()
        {
            var teacherId = HttpContext.Session.GetInt32("UserID");
            var teacherName = HttpContext.Session.GetString("UserName");
            var teacherEmail = HttpContext.Session.GetString("UserEmail");

            if (teacherId == null)
                return RedirectToAction("LoginTeacher", "Auth");

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/byTeacher/{teacherId}");
            var courseList = new List<CourseView>();

            if (courseRes.IsSuccessStatusCode)
            {
                var json = await courseRes.Content.ReadAsStringAsync();
                courseList = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();
            }

            var vm = new TeacherDashboardView
            {
                TeacherID = teacherId.Value,
                TeacherName = teacherName ?? "",
                TeacherEmail = teacherEmail ?? "",
                TotalCourses = courseList.Count,
                Courses = courseList
            };

            return View("~/Views/Teacher/Dashboard.cshtml", vm);
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetails(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Courses/details/{id}");

            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var course = JsonConvert.DeserializeObject<CourseView>(json);

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
    }
}
