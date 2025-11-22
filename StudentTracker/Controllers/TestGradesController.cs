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
            if (courseId == null)
                return RedirectToAction("Dashboard", "Teacher");

            ViewBag.CourseID = courseId;

            var gradeRes = await _client.GetAsync($"{_apiBase}TestGrades/byCourse/{courseId}");
            var grades = new List<TestGradeView>();

            if (gradeRes.IsSuccessStatusCode)
            {
                var json = await gradeRes.Content.ReadAsStringAsync();
                grades = JsonConvert.DeserializeObject<List<TestGradeView>>(json) ?? new();
            }

            // 🔹 Load course name for UI
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var courseJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(courseJson);
                ViewBag.CourseName = course?.CourseName ?? "";
            }

            // 🔹 Load test name (if grades exist)
            if (grades.Any())
            {
                int testId = grades.First().TestID;
                var testRes = await _client.GetAsync($"{_apiBase}Tests/{testId}");
                if (testRes.IsSuccessStatusCode)
                {
                    var json = await testRes.Content.ReadAsStringAsync();
                    var test = JsonConvert.DeserializeObject<TestView>(json);

                    ViewBag.TestName = test?.TestName ?? "";
                }
            }

            return View(grades);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            ViewBag.CourseID = courseId;

            // ---------------- Load Course Name ----------------
            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (courseRes.IsSuccessStatusCode)
            {
                var cJson = await courseRes.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<CourseView>(cJson);
                ViewBag.CourseName = course?.CourseName;
            }

            // ---------------- Load Tests of This Course ----------------
            var tests = new List<TestView>();
            var testsRes = await _client.GetAsync($"{_apiBase}Tests/byCourse/{courseId}");
            if (testsRes.IsSuccessStatusCode)
            {
                var json = await testsRes.Content.ReadAsStringAsync();
                tests = JsonConvert.DeserializeObject<List<TestView>>(json) ?? new();
            }
            ViewBag.Tests = tests;

            // ---------------- Load Students Enrolled in This Course ----------------
            var students = new List<StudentCourseView>();
            var studRes = await _client.GetAsync($"{_apiBase}StudentCourses/byCourse/{courseId}");
            if (studRes.IsSuccessStatusCode)
            {
                var json = await studRes.Content.ReadAsStringAsync();
                students = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();
            }
            ViewBag.Students = students;

            // ---------------- Return Model ----------------
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
        public async Task<IActionResult> BulkCreate(int? courseId, int? testId)
        {
            if (testId == null)
            {
                TempData["Msg"] = "Test not found.";
                return RedirectToAction(nameof(Index), new { courseId });
            }

            var testRes = await _client.GetAsync($"{_apiBase}Tests/{testId}");
            if (!testRes.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index), new { courseId });

            var testJson = await testRes.Content.ReadAsStringAsync();
            var test = JsonConvert.DeserializeObject<TestView>(testJson);
            if (test == null)
                return RedirectToAction(nameof(Index), new { courseId });

            ViewBag.TestName = test.TestName;
            ViewBag.CourseID = test.CourseID;

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
                AvailableTests = new List<TestView>(), // not needed when coming from test page
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
            return RedirectToAction(nameof(Index), new { courseId = model.CourseID });
        }
    }
}
