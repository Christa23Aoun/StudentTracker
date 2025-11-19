using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GradesController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public GradesController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? course, string? student, string? search)
        {
            var model = new List<AdminPendingGradeView>();

            try
            {
                var res = await _client.GetAsync($"{_apiBase}Dashboard/PendingGrades");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var list = JsonConvert.DeserializeObject<List<AdminPendingGradeView>>(json);
                    if (list != null)
                    {
                        model = list;
                    }
                }

                if (!string.IsNullOrWhiteSpace(course))
                {
                    model = model
                        .Where(g => string.Equals(g.CourseName, course, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(student))
                {
                    model = model
                        .Where(g => string.Equals(g.StudentName, student, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.ToLower();
                    model = model
                        .Where(g =>
                            (g.CourseName ?? string.Empty).ToLower().Contains(term) ||
                            (g.StudentName ?? string.Empty).ToLower().Contains(term) ||
                            (g.TestName ?? string.Empty).ToLower().Contains(term))
                        .ToList();
                }

                ViewBag.CourseFilter = course ?? string.Empty;
                ViewBag.StudentFilter = student ?? string.Empty;
                ViewBag.SearchTerm = search ?? string.Empty;

                ViewBag.Courses = model
                    .Select(g => g.CourseName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                ViewBag.Students = model
                    .Select(g => g.StudentName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Server error: {ex.Message}";
            }

            return View("~/Views/Grades/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Validate(int id, string? course, string? student, string? search)
        {
            await _client.PostAsync($"{_apiBase}Dashboard/ValidateGrade/{id}", null);
            return RedirectToAction("Index", new { course, student, search });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? course, string? student, string? search)
        {
            await _client.PostAsync($"{_apiBase}Dashboard/RejectGrade/{id}", null);
            return RedirectToAction("Index", new { course, student, search });
        }
    }
}
