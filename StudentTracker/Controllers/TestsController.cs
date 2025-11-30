using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        public async Task<IActionResult> Index(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var list = new List<TestView>();

            if (courseId.HasValue)
            {
                var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
                if (courseRes.IsSuccessStatusCode)
                {
                    var jsonCourse = await courseRes.Content.ReadAsStringAsync();
                    var course = JsonConvert.DeserializeObject<CourseView>(jsonCourse);
                    ViewBag.CourseName = course?.CourseName ?? "";
                }
            }

            string endpoint = courseId.HasValue
                ? $"{_apiBase}Tests/byCourse/{courseId}"
                : $"{_apiBase}Tests";

            var response = await _client.GetAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<TestView>>(json) ?? new();
            }

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            ViewBag.CourseID = courseId;

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            string courseName = "";

            if (courseRes.IsSuccessStatusCode)
            {
                var json = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(json);
                courseName = course?.CourseName ?? "";
            }

            ViewBag.CourseName = courseName;

            return View(new TestView { CourseID = courseId });
        }

        private async Task LoadCourseName(int courseId)
        {
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var json = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(json);
                ViewBag.CourseName = course?.CourseName ?? "";
            }

            ViewBag.CourseID = courseId;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestView model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCourseName(model.CourseID);
                return View(model);
            }

            var payload = new
            {
                CourseID = model.CourseID,
                TestName = model.TestName,
                TestDate = model.TestDate,
                Weight = model.Weight,
                MaxScore = model.MaxScore
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}Tests", content);

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "Test created successfully!";
                return RedirectToAction("Index", new { courseId = model.CourseID });
            }

            ViewBag.Error = await res.Content.ReadAsStringAsync();
            await LoadCourseName(model.CourseID);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Tests/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TestView>(json);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TestView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payload = new
            {
                TestID = model.TestID,
                CourseID = model.CourseID,
                TestName = model.TestName,
                TestDate = model.TestDate,
                Weight = model.Weight,
                MaxScore = model.MaxScore
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PutAsync($"{_apiBase}Tests", content);

            if (res.IsSuccessStatusCode)
                return RedirectToAction("Index", new { courseId = model.CourseID });

            ViewBag.Error = await res.Content.ReadAsStringAsync();
            return View(model);
        }

        // ================================
        // DELETE (GET)
        // ================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Load test
            var testRes = await _client.GetAsync($"{_apiBase}Tests/{id}");
            if (!testRes.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var testJson = await testRes.Content.ReadAsStringAsync();
            var test = JsonConvert.DeserializeObject<TestView>(testJson);

            if (test == null)
                return RedirectToAction(nameof(Index));

            // Load grades for validation check
            var gradeRes = await _client.GetAsync(
                $"{_apiBase}TestGrades/ByTest?courseId={test.CourseID}&testId={test.TestID}"
            );

            if (gradeRes.IsSuccessStatusCode)
            {
                var gJson = await gradeRes.Content.ReadAsStringAsync();
                var grades = JsonConvert.DeserializeObject<List<TestGradeView>>(gJson);

                if (grades != null && grades.Any(g => g.IsValidated))
                {
                    TempData["Error"] = "This test cannot be deleted because it has validated grades.";
                    return RedirectToAction(nameof(Index), new { courseId = test.CourseID });
                }
            }

            return View(test);
        }

        // ================================
        // DELETE (POST)
        // ================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int TestID, int CourseID)
        {
            // Double check before delete
            var gradeRes = await _client.GetAsync(
                $"{_apiBase}TestGrades/ByTest?courseId={CourseID}&testId={TestID}"
            );

            if (gradeRes.IsSuccessStatusCode)
            {
                var gJson = await gradeRes.Content.ReadAsStringAsync();
                var grades = JsonConvert.DeserializeObject<List<TestGradeView>>(gJson);

                if (grades != null && grades.Any(g => g.IsValidated))
                {
                    TempData["Error"] = "This test cannot be deleted because it has validated grades.";
                    return RedirectToAction(nameof(Index), new { courseId = CourseID });
                }
            }

            // If no validated grades → delete
            await _client.DeleteAsync($"{_apiBase}Tests/{TestID}");
            TempData["Msg"] = "Test deleted successfully.";

            return RedirectToAction(nameof(Index), new { courseId = CourseID });
        }
    }
}
