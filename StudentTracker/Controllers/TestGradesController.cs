using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class TestGradesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public TestGradesController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Index(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var list = new List<TestGradeView>();

            string endpoint = courseId.HasValue
                ? $"{_apiBase}TestGrades/byCourse/{courseId}"
                : $"{_apiBase}TestGrades";

            var res = await _client.GetAsync(endpoint);
            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<TestGradeView>>(json) ?? new();
            }

            return View(list);
        }

        public IActionResult Create(int? courseId)
        {
            ViewBag.CourseID = courseId;
            var model = new TestGradeView();

            if (courseId.HasValue)
                model.CourseID = courseId.Value;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestGradeView model)
        {
            model.IsValidated = false;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_apiBase}TestGrades", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Msg"] = "Test grade added successfully.";
                    return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
                }

                ViewBag.Error = "Failed to save test grade.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id, int? courseId)
        {
            ViewBag.CourseID = courseId;

            var res = await _client.GetAsync($"{_apiBase}TestGrades/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId });

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<TestGradeView>(json);
            if (item == null)
                return RedirectToAction(nameof(Index), new { courseId });

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? courseId)
        {
            var res = await _client.DeleteAsync($"{_apiBase}TestGrades/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "Test grade deleted successfully."
                : "Failed to delete test grade.";

            return RedirectToAction(nameof(Index), new { courseId });
        }

        [HttpGet]
        public async Task<IActionResult> BulkCreate(int courseId)
        {
            var students = new List<StudentCourseView>();
            var tests = new List<TestView>();

            var studentRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            if (studentRes.IsSuccessStatusCode)
            {
                var json = await studentRes.Content.ReadAsStringAsync();
                students = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();
            }

            var testsRes = await _client.GetAsync($"{_apiBase}Tests");
            if (testsRes.IsSuccessStatusCode)
            {
                var json = await testsRes.Content.ReadAsStringAsync();
                var allTests = JsonConvert.DeserializeObject<List<TestView>>(json) ?? new();
                tests = allTests.Where(t => t.CourseID == courseId).ToList();
            }

            var vm = new BulkTestGradesView
            {
                CourseID = courseId,
                AvailableTests = tests,
                Students = students
                    .Select(s => new StudentGradeInput
                    {
                        StudentID = s.StudentID,
                        StudentName = s.StudentName,
                        Score = null,
                        IsValidated = false
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(BulkTestGradesView model)
        {
            if (model.SelectedTestID <= 0)
                ModelState.AddModelError("SelectedTestID", "Please select a test.");

            if (!ModelState.IsValid)
            {
                var testsRes = await _client.GetAsync($"{_apiBase}Tests");
                if (testsRes.IsSuccessStatusCode)
                {
                    var json = await testsRes.Content.ReadAsStringAsync();
                    var allTests = JsonConvert.DeserializeObject<List<TestView>>(json) ?? new();
                    model.AvailableTests = allTests.Where(t => t.CourseID == model.CourseID).ToList();
                }

                return View(model);
            }

            foreach (var row in model.Students)
            {
                if (row.Score.HasValue)
                {
                    var grade = new TestGradeView
                    {
                        TestID = model.SelectedTestID,
                        StudentID = row.StudentID,
                        Score = row.Score.Value,
                        IsValidated = false,
                        CourseID = model.CourseID
                    };

                    var json = JsonConvert.SerializeObject(grade);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await _client.PostAsync($"{_apiBase}TestGrades", content);
                }
            }

            TempData["Msg"] = "Test grades saved successfully.";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
        }
    }
}
