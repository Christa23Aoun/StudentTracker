using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public TestsController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        private string ApiUrl(string relative)
        {
            var b = (_apiBase ?? "").Trim().TrimEnd('/');
            var r = (relative ?? "").Trim().TrimStart('/');

            if (b.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
                return $"{b}/{r}";

            return $"{b}/api/{r}";
        }

        private async Task<bool> EnsureTeacherAssigned(int courseId)
        {
            var teacherId = HttpContext.Session.GetInt32("UserID");
            if (teacherId == null)
                return false;

            var res = await _client.GetAsync(ApiUrl($"Courses/ByTeacher/{teacherId.Value}"));
            if (!res.IsSuccessStatusCode)
                return false;

            var json = await res.Content.ReadAsStringAsync();

            JArray courses;
            try
            {
                courses = JArray.Parse(json);
            }
            catch
            {
                return false;
            }

            return courses.Any(c =>
            {
                var id = c["courseID"] ?? c["CourseID"];
                return id != null && id.Value<int>() == courseId;
            });
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            string? pageError = null;

            if (TempData.ContainsKey("TestsIndexError"))
            {
                pageError = TempData["TestsIndexError"]?.ToString();
                TempData.Keep("TestsIndexError");
            }

            if (courseId.HasValue && pageError == null)
            {
                var allowed = await EnsureTeacherAssigned(courseId.Value);
                if (!allowed)
                {
                    TempData["Error"] = "You are no longer assigned to this course and cannot view its content.";
                    return Redirect("/Teacher/Dashboard");
                }
            }

            ViewBag.Error = pageError;
            ViewBag.CourseID = courseId;

            var list = new List<TestView>();

            if (courseId.HasValue)
            {
                var courseRes = await _client.GetAsync(ApiUrl($"Courses/{courseId.Value}"));
                if (courseRes.IsSuccessStatusCode)
                {
                    var jsonCourse = await courseRes.Content.ReadAsStringAsync();
                    var course = JsonConvert.DeserializeObject<CourseView>(jsonCourse);
                    ViewBag.CourseName = course?.CourseName ?? "";
                }
            }

            var endpoint = courseId.HasValue
                ? ApiUrl($"Tests/byCourse/{courseId.Value}")
                : ApiUrl("Tests");

            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<TestView>>(json) ?? new();
            }

            foreach (var t in list)
            {
                var avgRes = await _client.GetAsync(ApiUrl($"TestGrades/AverageByTest/{t.TestID}"));
                t.AverageScore = avgRes.IsSuccessStatusCode
                    ? JsonConvert.DeserializeObject<decimal>(await avgRes.Content.ReadAsStringAsync())
                    : 0;
            }

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            if (!await EnsureTeacherAssigned(courseId))
            {
                TempData["Error"] = "You are no longer assigned to this course.";
                return Redirect("/Teacher/Dashboard");
            }

            ViewBag.CourseID = courseId;

            var courseRes = await _client.GetAsync(ApiUrl($"Courses/{courseId}"));
            if (courseRes.IsSuccessStatusCode)
            {
                var course = JsonConvert.DeserializeObject<CourseView>(
                    await courseRes.Content.ReadAsStringAsync());
                ViewBag.CourseName = course?.CourseName ?? "";
            }

            return View(new TestView { CourseID = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestView model)
        {
            if (!await EnsureTeacherAssigned(model.CourseID))
            {
                TempData["Error"] = "You are no longer assigned to this course.";
                return Redirect("/Teacher/Dashboard");
            }

            if (!ModelState.IsValid)
                return View(model);

            var json = JsonConvert.SerializeObject(model);
            var res = await _client.PostAsync(
                ApiUrl("Tests"),
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                return View(model);
            }

            TempData["Msg"] = "Test created successfully!";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _client.GetAsync(ApiUrl($"Tests/{id}"));
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var model = JsonConvert.DeserializeObject<TestView>(
                await res.Content.ReadAsStringAsync());

            if (model == null || !await EnsureTeacherAssigned(model.CourseID))
            {
                TempData["Error"] = "Access denied.";
                return Redirect("/Teacher/Dashboard");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TestView model)
        {
            if (!await EnsureTeacherAssigned(model.CourseID))
            {
                TempData["Error"] = "Access denied.";
                return Redirect("/Teacher/Dashboard");
            }

            var json = JsonConvert.SerializeObject(model);
            var res = await _client.PutAsync(
                ApiUrl("Tests"),
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = await res.Content.ReadAsStringAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _client.GetAsync(ApiUrl($"Tests/{id}"));
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var model = JsonConvert.DeserializeObject<TestView>(
                await res.Content.ReadAsStringAsync());

            if (model == null || !await EnsureTeacherAssigned(model.CourseID))
            {
                TempData["Error"] = "Access denied.";
                return Redirect("/Teacher/Dashboard");
            }

            return View(model);
        }
       
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(TestView model)
        {
            var allowed = await EnsureTeacherAssigned(model.CourseID);
            if (!allowed)
            {
                TempData["Error"] = "You are no longer assigned to this course.";
                return Redirect("/Teacher/Dashboard");
            }

            var res = await _client.DeleteAsync(ApiUrl($"Tests/{model.TestID}"));

            if (!res.IsSuccessStatusCode)
            {
                TempData["TestsIndexError"] = await res.Content.ReadAsStringAsync();
                return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
            }

            TempData["Msg"] = "Test deleted successfully.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
        }



    }
}
