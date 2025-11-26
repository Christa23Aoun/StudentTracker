using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;
using System.Text.Json;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class StudentCoursesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public StudentCoursesController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}StudentCourses");
            if (!res.IsSuccessStatusCode)
                return View(new List<StudentCourseView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentCourseView>>(json) ?? new();
            return View(data);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentCourseView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}StudentCourses", content);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to add student course record.");
                return View(model);
            }

            TempData["Msg"] = "✅ Student course created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}StudentCourses/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<StudentCourseView>(json);
            if (item == null)
                return RedirectToAction(nameof(Index));

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = await _client.DeleteAsync($"{_apiBase}StudentCourses/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "✅ Student course deleted successfully."
                : "⚠️ Failed to delete student course.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Enroll(int studentId)
        {
            var studentRes = await _client.GetAsync($"{_apiBase}Users/{studentId}");
            if (!studentRes.IsSuccessStatusCode)
                return View("Error");

            var studentJson = await studentRes.Content.ReadAsStringAsync();
            var student = JsonConvert.DeserializeObject<UserView>(studentJson);

            var depRes = await _client.GetAsync($"{_apiBase}Departments");
            if (!depRes.IsSuccessStatusCode)
                return View("Error");

            var depJson = await depRes.Content.ReadAsStringAsync();
            var deps = JsonConvert.DeserializeObject<List<DepartmentView>>(depJson) ?? new();

            var departmentCourses = new List<DepartmentCoursesView>();
            foreach (var d in deps)
            {
                var depCourses = new DepartmentCoursesView
                {
                    DepartmentName = d.DepartmentName,
                    Courses = new List<SimpleCourseView>()
                };

                if (d.Courses != null)
                {
                    foreach (var c in d.Courses)
                    {
                        depCourses.Courses.Add(new SimpleCourseView
                        {
                            CourseID = c.CourseID,
                            CourseName = c.CourseName,
                            DepartmentName = d.DepartmentName
                        });
                    }
                 }

                departmentCourses.Add(depCourses);
            }

            var model = new EnrollmentViewModel
            {
                StudentID = studentId,
                StudentName = student?.FullName ?? "Unknown Student",
                Departments = departmentCourses
            };

            return View("~/Views/Enrollments/Enroll.cshtml", model);
        }
    }
}
