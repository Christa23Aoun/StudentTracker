using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using StudentTracker.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class StudentDashboardController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public StudentDashboardController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value
                       ?? "https://localhost:7199/api/";
        }

        public async Task<IActionResult> Index(int? studentId, string? semester, string? department)
        {
            var id = studentId ?? HttpContext.Session.GetInt32("StudentId");
            if (id == null)
                return RedirectToAction("Login", "Auth");

            var model = new StudentDashboardVM();

            try
            {
                var endpoint = $"{_apiBase}Dashboard/Student/{id}";
                var response = await _client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                    return View(model);

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<StudentDashboardVM>(json) ?? new StudentDashboardVM();
            }
            catch
            {
                return View(new StudentDashboardVM());
            }

            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            var id = HttpContext.Session.GetInt32("StudentId");
            if (id == null)
                return RedirectToAction("Login", "Auth");

            var model = new StudentDashboardVM();

            try
            {
                var endpoint = $"{_apiBase}Dashboard/Student/{id}";
                var response = await _client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                    return View(model);

                var json = await response.Content.ReadAsStringAsync();
                model = JsonConvert.DeserializeObject<StudentDashboardVM>(json) ?? new StudentDashboardVM();
            }
            catch
            {
                return View(new StudentDashboardVM());
            }

            return View(model);
        }

        public async Task<IActionResult> CourseDetails(int courseId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "Auth");

            var vm = new StudentCourseDetailsVM();

            try
            {
                var endpoint = $"{_apiBase}Dashboard/Student/{studentId}/course/{courseId}";
                var res = await _client.GetAsync(endpoint);
                if (!res.IsSuccessStatusCode)
                    return View(vm);

                var json = await res.Content.ReadAsStringAsync();
                vm = JsonConvert.DeserializeObject<StudentCourseDetailsVM>(json) ?? new StudentCourseDetailsVM();
            }
            catch
            {
                return View(new StudentCourseDetailsVM());
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Schedule(int? semesterId, DateTime? weekStart)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
                return RedirectToAction("Login", "Auth");

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

            DateTime start;
            if (weekStart.HasValue)
            {
                start = weekStart.Value.Date;
            }
            else
            {
                var today = DateTime.Today;
                int diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                start = today.AddDays(-diff);
            }

            var end = start.AddDays(6);

            var endpoint =
                $"{_apiBase}Dashboard/Student/{studentId}/schedule" +
                $"?semesterId={selectedSemesterId}" +
                $"&weekStart={start:yyyy-MM-dd}" +
                $"&weekEnd={end:yyyy-MM-dd}";

            var response = await _client.GetAsync(endpoint);

            var schedule = new List<StudentScheduleItemView>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                schedule = JsonConvert.DeserializeObject<List<StudentScheduleItemView>>(json) ?? new();
            }

            ViewBag.SemesterOptions = semesterOptions;
            ViewBag.SelectedSemesterId = selectedSemesterId;
            ViewBag.WeekStart = start;
            ViewBag.WeekEnd = end;

            return View(schedule);
        }
    }
}
