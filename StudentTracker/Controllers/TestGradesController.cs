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

        // ===============================
        // INDEX
        // ===============================
        public async Task<IActionResult> Index(int? courseId, int? testId)
        {
            if (courseId == null)
                return RedirectToAction("Dashboard", "Teacher");

            ViewBag.CourseID = courseId;

            // Course
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var json = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(json);
                ViewBag.CourseName = course?.CourseName ?? "";
            }

            // Tests
            var testsRes = await _client.GetAsync($"{_apiBase}Tests/byCourse/{courseId}");
            var tests = new List<TestView>();

            if (testsRes.IsSuccessStatusCode)
            {
                var tJson = await testsRes.Content.ReadAsStringAsync();
                tests = JsonConvert.DeserializeObject<List<TestView>>(tJson) ?? new();
            }
            ViewBag.Tests = tests;

            if (testId == null || testId <= 0)
            {
                ViewBag.SelectedTestID = 0;
                return View(new List<TestGradeView>());
            }

            ViewBag.SelectedTestID = testId;

            // Test Name
            var testRes = await _client.GetAsync($"{_apiBase}Tests/{testId}");
            if (testRes.IsSuccessStatusCode)
            {
                var json = await testRes.Content.ReadAsStringAsync();
                var test = JsonConvert.DeserializeObject<TestView>(json);
                ViewBag.TestName = test?.TestName ?? "";
            }

            // Grades
            var gradesRes = await _client.GetAsync($"{_apiBase}TestGrades/ByTest?courseId={courseId}&testId={testId}");
            var grades = new List<TestGradeView>();

            if (gradesRes.IsSuccessStatusCode)
            {
                var json = await gradesRes.Content.ReadAsStringAsync();
                grades = JsonConvert.DeserializeObject<List<TestGradeView>>(json) ?? new();
            }

            return View(grades);
        }


        // ===============================
        // DELETE GET
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Delete(int id, int courseId, int testId)
        {
            var res = await _client.GetAsync($"{_apiBase}TestGrades/{id}");

            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId, testId });

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TestGradeView>(json);

            if (model == null)
                return RedirectToAction(nameof(Index), new { courseId, testId });

            if (model.IsValidated)
            {
                TempData["Error"] = "You cannot delete a validated grade.";
                return RedirectToAction(nameof(Index), new { courseId, testId });
            }

            // Fetch test name
            var testRes = await _client.GetAsync($"{_apiBase}Tests/{model.TestID}");
            if (testRes.IsSuccessStatusCode)
            {
                var tJson = await testRes.Content.ReadAsStringAsync();
                var test = JsonConvert.DeserializeObject<TestView>(tJson);
                model.TestName = test?.TestName;
            }

            // Fetch course name
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var cJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(cJson);
                model.CourseName = course?.CourseName;
            }

            ViewBag.CourseID = courseId;
            ViewBag.TestID = testId;

            return View(model);
        }


        // ===============================
        // DELETE POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int courseId, int testId)
        {
            await _client.DeleteAsync($"{_apiBase}TestGrades/{id}");

            return RedirectToAction(nameof(Index), new { courseId, testId });
        }


        // ===============================
        // CREATE GET
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            ViewBag.CourseID = courseId;

            // Course
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var cJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(cJson);
                ViewBag.CourseName = course?.CourseName;
            }

            // Tests
            var testsRes = await _client.GetAsync($"{_apiBase}Tests/byCourse/{courseId}");
            ViewBag.Tests = testsRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<TestView>>(await testsRes.Content.ReadAsStringAsync()) ?? new()
                : new List<TestView>();

            // Students
            var studRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            ViewBag.Students = studRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<StudentCourseView>>(await studRes.Content.ReadAsStringAsync()) ?? new()
                : new List<StudentCourseView>();

            return View(new TestGradeView
            {
                CourseID = courseId,
                IsValidated = false
            });
        }


        // ===============================
        // CREATE POST
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestGradeView model)
        {
            model.IsValidated = false;

            if (!ModelState.IsValid)
                return View(model);

            // Convert Score correctly → decimal
            decimal finalScore = Convert.ToDecimal(model.Score);

            var payload = new
            {
                TestID = model.TestID,
                StudentID = model.StudentID,
                Score = finalScore,
                IsValidated = false
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync($"{_apiBase}TestGrades", content);

            if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                TempData["Error"] = "This student already has a grade for this test!";
                return RedirectToAction(nameof(Create), new { courseId = model.CourseID });
            }

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "Test grade added successfully.";
                return RedirectToAction(nameof(Index), new { courseId = model.CourseID, testId = model.TestID });
            }

            ViewBag.Error = "Failed to save test grade.";
            return View(model);
        }


        // ===============================
        // BULK CREATE
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(BulkTestGradesView model)
        {
            if (model.SelectedTestID <= 0)
            {
                ModelState.AddModelError("SelectedTestID", "Test ID missing.");
                return View(model);
            }

            foreach (var row in model.Students)
            {
                if (row.Score.HasValue)
                {
                    decimal finalScore = Convert.ToDecimal(row.Score.Value);

                    var payload = new
                    {
                        TestID = model.SelectedTestID,
                        StudentID = row.StudentID,
                        Score = finalScore,
                        IsValidated = false
                    };

                    var json = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var post = await _client.PostAsync($"{_apiBase}TestGrades", content);

                    if (post.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        TempData["Error"] = $"Student {row.StudentName} already has a grade for this test!";
                        return RedirectToAction(nameof(BulkCreate), new { courseId = model.CourseID, testId = model.SelectedTestID });
                    }
                }
            }

            TempData["Msg"] = "Grades added successfully!";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseID, testId = model.SelectedTestID });
        }
    }
}
