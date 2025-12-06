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
            var viewModel = new GenerateSessionsPageView
            {
                CourseID = courseId
            };

            var cRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (cRes.IsSuccessStatusCode)
            {
                var jsonCourse = await cRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(jsonCourse);
                if (course != null)
                {
                    viewModel.CourseName = course.CourseName;
                   
                }
            }

            var sRes = await _client.GetAsync($"{_apiBase}CourseSessions/course/{courseId}");
            if (sRes.IsSuccessStatusCode)
            {
                var jsonSessions = await sRes.Content.ReadAsStringAsync();
                viewModel.Sessions = JsonConvert.DeserializeObject<List<CourseSessionView>>(jsonSessions) ?? new();
            }

            return View("Sessions", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSessions(GenerateSessionsRequest req)
        {
            var payload = JsonConvert.SerializeObject(req);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync($"{_apiBase}CourseSessions/generate", content);

            if (!res.IsSuccessStatusCode)
                TempData["CourseError"] = "Failed to generate sessions.";
            else
                TempData["CourseSuccess"] = "Sessions generated successfully.";

            return RedirectToAction("Sessions", new { courseId = req.CourseID });
        }


    }
}
