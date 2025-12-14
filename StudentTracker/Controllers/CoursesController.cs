using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StudentTracker.Controllers
{
    public class CoursesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public CoursesController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}Courses");
            if (!res.IsSuccessStatusCode)
                return View(new List<CourseView>());

            var json = await res.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Sessions(int courseId)
        {
            var vm = new GenerateSessionsPageView
            {
                CourseID = courseId
            };

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (!courseRes.IsSuccessStatusCode)
                return View(vm);

            var courseJson = await courseRes.Content.ReadAsStringAsync();
            var course = JsonConvert.DeserializeObject<CourseView>(courseJson);
            if (course == null)
                return View(vm);

            vm.CourseName = course.CourseName;

            var semesterRes = await _client.GetAsync($"{_apiBase}Semesters/{course.SemesterID}");
            if (!semesterRes.IsSuccessStatusCode)
                return View(vm);

            var semesterJson = await semesterRes.Content.ReadAsStringAsync();
            var semester = JsonConvert.DeserializeObject<SemesterView>(semesterJson);
            if (semester == null)
                return View(vm);

            vm.SemesterStartDate = semester.StartDate.Date;
            vm.SemesterEndDate = semester.EndDate.Date;

            var sessionsRes = await _client.GetAsync($"{_apiBase}CourseSessions/course/{courseId}");
            if (sessionsRes.IsSuccessStatusCode)
            {
                var sessionsJson = await sessionsRes.Content.ReadAsStringAsync();
                vm.Sessions = JsonConvert.DeserializeObject<List<CourseSessionView>>(sessionsJson) ?? new();
            }

            return View("Sessions", vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSessions(GenerateSessionsRequest req)
        {
            var payload = JsonConvert.SerializeObject(req);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync($"{_apiBase}CourseSessions/generate", content);

            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                TempData["CourseError"] = msg;
            }
            else
            {
                TempData["CourseSuccess"] = "Sessions generated successfully.";
            }

            return RedirectToAction("Sessions", new { courseId = req.CourseID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(int sessionId, int courseId)
        {
            var res = await _client.DeleteAsync($"{_apiBase}CourseSessions/{sessionId}");

            if (!res.IsSuccessStatusCode)
                TempData["CourseError"] = "Failed to delete session.";
            else
                TempData["CourseSuccess"] = "Session deleted successfully.";

            return RedirectToAction("Sessions", new { courseId });
        }
    }
}
