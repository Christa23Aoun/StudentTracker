using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;
using System.Linq;

namespace StudentTracker.Controllers
{
    public class CoursesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public CoursesController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("API");

            var baseUrl = config.GetSection("ApiSettings:BaseUrl").Value ?? "";
            _apiBase = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
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

        private async Task PopulateLookupsAsync(CourseView model)
        {
            var depRes = await _client.GetAsync($"{_apiBase}Lookups/departments");
            model.Departments = depRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<LookupItem>>(await depRes.Content.ReadAsStringAsync()) ?? new()
                : new();

            var semRes = await _client.GetAsync($"{_apiBase}Lookups/semesters");
            model.Semesters = semRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<LookupItem>>(await semRes.Content.ReadAsStringAsync()) ?? new()
                : new();

            var teacherRes = await _client.GetAsync($"{_apiBase}Lookups/teachers");
            model.Teachers = teacherRes.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<LookupItem>>(await teacherRes.Content.ReadAsStringAsync()) ?? new()
                : new();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CourseView { IsActive = true };
            await PopulateLookupsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseView model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(model);
                return View(model);
            }

            var cleanPayload = new
            {
                model.CourseCode,
                model.CourseName,
                model.CreditHours,
                model.DepartmentID,
                model.SemesterID,
                model.TeacherID,
                model.IsActive
            };

            var payload = JsonConvert.SerializeObject(cleanPayload);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var res = await _client.PostAsync($"{_apiBase}Courses", content);

            if (!res.IsSuccessStatusCode)
            {
                var apiMsg = await res.Content.ReadAsStringAsync();
                TempData["CourseError"] = string.IsNullOrWhiteSpace(apiMsg) ? "Failed to create course." : apiMsg;
                await PopulateLookupsAsync(model);
                return View(model);
            }

            TempData["CourseSuccess"] = "Course created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Courses/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<CourseView>(json);

            if (model == null)
                return RedirectToAction(nameof(Index));

            await PopulateLookupsAsync(model);

            if (model.DepartmentID == 0 && model.Departments.Any())
                model.DepartmentID = model.Departments.First().Id;

            if (model.SemesterID == 0 && model.Semesters.Any())
                model.SemesterID = model.Semesters.First().Id;

            if (model.TeacherID == 0 && model.Teachers.Any())
                model.TeacherID = model.Teachers.First().Id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseView model)
        {
            var finalId = id > 0 ? id : model.CourseID;

            if (finalId <= 0)
            {
                TempData["CourseError"] = "Invalid course id.";
                await PopulateLookupsAsync(model);
                return View(model);
            }

            model.CourseID = finalId;

            if (!ModelState.IsValid)
            {
                TempData["CourseError"] = "Please fix the validation errors and try again.";
                await PopulateLookupsAsync(model);
                return View(model);
            }

            var cleanPayload = new
            {
                CourseID = finalId,
                model.CourseCode,
                model.CourseName,
                model.CreditHours,
                model.DepartmentID,
                model.SemesterID,
                model.TeacherID,
                model.IsActive
            };

            var payload = JsonConvert.SerializeObject(cleanPayload);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var res = await _client.PutAsync($"{_apiBase}Courses/{finalId}", content);
            if (!res.IsSuccessStatusCode)
            {
                var apiMsg = await res.Content.ReadAsStringAsync();

                TempData["CourseError"] =
                    $"API ERROR | Status: {(int)res.StatusCode} {res.StatusCode} | Message: {apiMsg}";

                await PopulateLookupsAsync(model);
                return View(model);
            }

            TempData["CourseSuccess"] = "Course updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> DeactivateCourseConfirmation(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Courses/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var course = JsonConvert.DeserializeObject<CourseView>(json);

            if (course == null)
                return RedirectToAction(nameof(Index));

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateCourse(int courseId)
        {
            var res = await _client.PutAsync($"{_apiBase}Courses/deactivate/{courseId}", null);

            TempData["CourseSuccess"] = res.IsSuccessStatusCode
                ? "Course deactivated successfully."
                : "Failed to deactivate course.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Sessions(int courseId)
        {
            var vm = new GenerateSessionsPageView { CourseID = courseId };

            var courseRes = await _client.GetAsync($"{_apiBase}Courses/{courseId}");
            if (!courseRes.IsSuccessStatusCode)
                return View("Sessions", vm);

            var courseJson = await courseRes.Content.ReadAsStringAsync();
            var course = JsonConvert.DeserializeObject<CourseView>(courseJson);
            if (course == null)
                return View("Sessions", vm);

            vm.CourseName = course.CourseName;
            vm.TeacherName = course.TeacherName ?? "";

            var semesterRes = await _client.GetAsync($"{_apiBase}Semesters/{course.SemesterID}");
            if (semesterRes.IsSuccessStatusCode)
            {
                var semesterJson = await semesterRes.Content.ReadAsStringAsync();
                var semester = JsonConvert.DeserializeObject<SemesterView>(semesterJson);
                if (semester != null)
                {
                    vm.SemesterStartDate = semester.StartDate.Date;
                    vm.SemesterEndDate = semester.EndDate.Date;
                }
            }

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

            TempData["CourseSuccess"] = res.IsSuccessStatusCode
                ? "Sessions generated successfully."
                : await res.Content.ReadAsStringAsync();

            return RedirectToAction(nameof(Sessions), new { courseId = req.CourseID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(int sessionId, int courseId)
        {
            var res = await _client.DeleteAsync($"{_apiBase}CourseSessions/{sessionId}");

            TempData["CourseSuccess"] = res.IsSuccessStatusCode
                ? "Session deleted successfully."
                : "Failed to delete session.";

            return RedirectToAction(nameof(Sessions), new { courseId });
        }
    }
}
