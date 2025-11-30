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

        // ============================================
        // SMALL HELPER FOR CREATE VIEW DROPDOWNS
        // ============================================
        private async Task LoadCreateDropdowns(int courseId)
        {
            ViewBag.CourseID = courseId;

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var cJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(cJson);
                ViewBag.CourseName = course?.CourseName;
            }

            var testsRes = await _client.GetAsync($"{_apiBase}Tests/byCourse/{courseId}");
            ViewBag.Tests = testsRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<TestView>>(await testsRes.Content.ReadAsStringAsync()) ?? new()
                : new List<TestView>();

            var studRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            ViewBag.Students = studRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<StudentCourseView>>(await studRes.Content.ReadAsStringAsync()) ?? new()
                : new List<StudentCourseView>();
        }

        // ============================================
        // INDEX
        // ============================================
        public async Task<IActionResult> Index(int? courseId, int? testId)
        {
            if (courseId == null)
                return RedirectToAction("Dashboard", "Teacher");

            ViewBag.CourseID = courseId;

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var json = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(json);
                ViewBag.CourseName = course?.CourseName ?? "";
            }

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

            var testRes = await _client.GetAsync($"{_apiBase}Tests/{testId}");
            if (testRes.IsSuccessStatusCode)
            {
                var json = await testRes.Content.ReadAsStringAsync();
                var test = JsonConvert.DeserializeObject<TestView>(json);
                ViewBag.TestName = test?.TestName ?? "";
            }

            var gradesRes = await _client.GetAsync($"{_apiBase}TestGrades/ByTest?courseId={courseId}&testId={testId}");
            var grades = new List<TestGradeView>();

            if (gradesRes.IsSuccessStatusCode)
            {
                var json = await gradesRes.Content.ReadAsStringAsync();
                grades = JsonConvert.DeserializeObject<List<TestGradeView>>(json) ?? new();
            }

            return View(grades);
        }

        // ============================================
        // DELETE (GET)
        // ============================================
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

            var testRes = await _client.GetAsync($"{_apiBase}Tests/{model.TestID}");
            if (testRes.IsSuccessStatusCode)
            {
                var tJson = await testRes.Content.ReadAsStringAsync();
                var test = JsonConvert.DeserializeObject<TestView>(tJson);
                model.TestName = test?.TestName;
            }

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

        // ============================================
        // DELETE (POST)
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int courseId, int testId)
        {
            await _client.DeleteAsync($"{_apiBase}TestGrades/{id}");
            return RedirectToAction(nameof(Index), new { courseId, testId });
        }

        // ============================================
        // CREATE (GET)
        // ============================================
        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            await LoadCreateDropdowns(courseId);

            return View(new TestGradeView
            {
                CourseID = courseId,
                IsValidated = false
            });
        }

        // ============================================
        // CREATE (POST) — FIXED WITH EXISTING GRADE LOGIC
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestGradeView model)
        {
            model.IsValidated = false;

            if (!ModelState.IsValid)
            {
                await LoadCreateDropdowns(model.CourseID);
                return View(model);
            }

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

            // ===========================
            // NEW: HANDLE DUPLICATE GRADE
            // ===========================
            if (res.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // Load existing grades
                var existingRes = await _client.GetAsync(
                    $"{_apiBase}TestGrades/ByTest?courseId={model.CourseID}&testId={model.TestID}");

                var grades = new List<TestGradeView>();
                if (existingRes.IsSuccessStatusCode)
                {
                    var jsonEx = await existingRes.Content.ReadAsStringAsync();
                    grades = JsonConvert.DeserializeObject<List<TestGradeView>>(jsonEx) ?? new();
                }

                var existing = grades.FirstOrDefault(g => g.StudentID == model.StudentID);

                ViewBag.AlreadyGraded = true;
                ViewBag.ExistingGrade = existing?.Score;

                ModelState.AddModelError(string.Empty, "This student already has a grade for this test!");

                await LoadCreateDropdowns(model.CourseID);
                return View(model);
            }

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "Test grade added successfully.";
                return RedirectToAction(nameof(Index), new { courseId = model.CourseID, testId = model.TestID });
            }

            var apiError = await res.Content.ReadAsStringAsync();
            ViewBag.Error = $"Failed to save test grade. API said: {apiError}";
            await LoadCreateDropdowns(model.CourseID);
            return View(model);
        }

        // ============================================
        // BULK CREATE (GET)
        // ============================================
        [HttpGet]
        public async Task<IActionResult> BulkCreate(int courseId, int testId)
        {
            var testRes = await _client.GetAsync($"{_apiBase}Tests/{testId}");
            if (!testRes.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId });

            var testJson = await testRes.Content.ReadAsStringAsync();
            var test = JsonConvert.DeserializeObject<TestView>(testJson);
            if (test == null)
                return RedirectToAction(nameof(Index), new { courseId });

            ViewBag.TestName = test.TestName;
            ViewBag.CourseID = courseId;

            var cRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (cRes.IsSuccessStatusCode)
            {
                var cJson = await cRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(cJson);
                ViewBag.CourseName = course?.CourseName ?? "";
            }

            var sRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            var students = sRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<StudentCourseView>>(await sRes.Content.ReadAsStringAsync()) ?? new()
                : new List<StudentCourseView>();

            var gRes = await _client.GetAsync($"{_apiBase}TestGrades/ByTest?courseId={courseId}&testId={testId}");
            var existing = gRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<TestGradeView>>(await gRes.Content.ReadAsStringAsync()) ?? new()
                : new List<TestGradeView>();

            var vm = new BulkTestGradesView
            {
                CourseID = courseId,
                SelectedTestID = testId,
                Students = students.Select(s =>
                {
                    var grade = existing.FirstOrDefault(g => g.StudentID == s.StudentID);

                    return new StudentGradeInput
                    {
                        StudentID = s.StudentID,
                        StudentName = s.StudentName,
                        Score = grade != null ? (double?)Convert.ToDouble(grade.Score) : null,
                        IsValidated = grade != null
                    };
                }).ToList()
            };

            return View(vm);
        }

        // ============================================
        // BULK CREATE (POST)
        // ============================================
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
                if (row.IsValidated)
                    continue;

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
                        return RedirectToAction(nameof(BulkCreate),
                            new { courseId = model.CourseID, testId = model.SelectedTestID });
                    }
                }
            }

            TempData["Msg"] = "Grades added successfully!";
            return RedirectToAction(nameof(Index),
                new { courseId = model.CourseID, testId = model.SelectedTestID });
        }
    }
}
