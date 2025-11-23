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

        // ===========================
        // INDEX — SHOW TEST DROPDOWN + GRADES
        // ===========================
        public async Task<IActionResult> Index(int? courseId, int? testId)
        {
            if (courseId == null)
                return RedirectToAction("Dashboard", "Teacher");

            ViewBag.CourseID = courseId;

            // -------- GET COURSE NAME --------
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var json = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(json);
                ViewBag.CourseName = course?.CourseName ?? "";
            }

            // -------- LOAD AVAILABLE TESTS FOR THIS COURSE --------
            var testsRes = await _client.GetAsync($"{_apiBase}Tests/byCourse/{courseId}");
            var tests = new List<TestView>();

            if (testsRes.IsSuccessStatusCode)
            {
                var tJson = await testsRes.Content.ReadAsStringAsync();
                tests = JsonConvert.DeserializeObject<List<TestView>>(tJson) ?? new();
            }
            ViewBag.Tests = tests;

            // If no test selected -> show empty page
            if (testId == null || testId <= 0)
            {
                ViewBag.SelectedTestID = 0;
                return View(new List<TestGradeView>());
            }

            ViewBag.SelectedTestID = testId;

            // -------- GET TEST NAME --------
            var testRes = await _client.GetAsync($"{_apiBase}Tests/{testId}");
            if (testRes.IsSuccessStatusCode)
            {
                var json = await testRes.Content.ReadAsStringAsync();
                var test = JsonConvert.DeserializeObject<TestView>(json);
                ViewBag.TestName = test?.TestName ?? "";
            }

            // -------- LOAD GRADES FOR THIS SPECIFIC TEST --------
            var gradeRes = await _client.GetAsync($"{_apiBase}TestGrades/byTest/{testId}");
            var grades = new List<TestGradeView>();

            if (gradeRes.IsSuccessStatusCode)
            {
                var json = await gradeRes.Content.ReadAsStringAsync();
                grades = JsonConvert.DeserializeObject<List<TestGradeView>>(json) ?? new();
            }

            return View(grades);
        }

        // ===========================
        // CREATE SINGLE TEST GRADE
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            ViewBag.CourseID = courseId;

            // Load course
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var cJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(cJson);
                ViewBag.CourseName = course?.CourseName;
            }

            // Load tests
            var tests = new List<TestView>();
            var testsRes = await _client.GetAsync($"{_apiBase}Tests/byCourse/{courseId}");
            if (testsRes.IsSuccessStatusCode)
            {
                var json = await testsRes.Content.ReadAsStringAsync();
                tests = JsonConvert.DeserializeObject<List<TestView>>(json) ?? new();
            }
            ViewBag.Tests = tests;

            // Load students
            var students = new List<StudentCourseView>();
            var studRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            if (studRes.IsSuccessStatusCode)
            {
                var json = await studRes.Content.ReadAsStringAsync();
                students = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();
            }
            ViewBag.Students = students;

            return View(new TestGradeView
            {
                CourseID = courseId,
                IsValidated = false
            });
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

                var res = await _client.PostAsync($"{_apiBase}TestGrades", content);

                if (res.IsSuccessStatusCode)
                {
                    TempData["Msg"] = "Test grade added successfully.";
                    return RedirectToAction(nameof(Index), new { courseId = model.CourseID, testId = model.TestID });
                }

                ViewBag.Error = "Failed to save test grade.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Server error: " + ex.Message;
            }

            return View(model);
        }

        // ===========================
        // DELETE (Instant delete)
        // ===========================
        [HttpGet]
        public async Task<IActionResult> Delete(int id, int? courseId, int? testId)
        {
            var res = await _client.DeleteAsync($"{_apiBase}TestGrades/{id}");

            TempData["Msg"] = res.IsSuccessStatusCode
                ? "Test grade deleted successfully."
                : "Failed to delete test grade.";

            return RedirectToAction(nameof(Index), new { courseId, testId });
        }

        // ===========================
        // BULK CREATE
        // ===========================
        [HttpGet]
        public async Task<IActionResult> BulkCreate(int? courseId, int? testId)
        {
            if (testId == null)
            {
                TempData["Msg"] = "Test not found.";
                return RedirectToAction(nameof(Index), new { courseId });
            }

            // Load test
            var testRes = await _client.GetAsync($"{_apiBase}Tests/{testId}");
            if (!testRes.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId });

            var testJson = await testRes.Content.ReadAsStringAsync();
            var test = JsonConvert.DeserializeObject<TestView>(testJson);
            if (test == null)
                return RedirectToAction(nameof(Index), new { courseId });

            ViewBag.TestName = test.TestName;
            ViewBag.CourseID = test.CourseID;

            // Load course name
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{test.CourseID}");
            if (courseRes.IsSuccessStatusCode)
            {
                var courseJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(courseJson);
                ViewBag.CourseName = course?.CourseName ?? $"Course {test.CourseID}";
            }

            // Load students
            var studentsRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{test.CourseID}");
            var students = new List<StudentCourseView>();

            if (studentsRes.IsSuccessStatusCode)
            {
                var json = await studentsRes.Content.ReadAsStringAsync();
                students = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();
            }

            var vm = new BulkTestGradesView
            {
                CourseID = test.CourseID,
                SelectedTestID = test.TestID,
                AvailableTests = new List<TestView>(),
                Students = students.Select(s => new StudentGradeInput
                {
                    StudentID = s.StudentID,
                    StudentName = s.StudentName,
                    Score = null,
                    IsValidated = false
                }).ToList()
            };

            return View(vm);
        }

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

            TempData["Msg"] = "Grades added successfully!";
            return RedirectToAction(nameof(Index), new { courseId = model.CourseID, testId = model.SelectedTestID });
        }
    }
}
