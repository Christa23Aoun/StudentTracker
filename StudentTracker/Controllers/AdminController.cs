using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace StudentTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public AdminController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient("API");
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        [HttpGet("/admin/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel();

            try
            {
                var summaryRes = await _client.GetAsync($"{_apiBase}Dashboard/AdminSummary");
                if (summaryRes.IsSuccessStatusCode)
                {
                    var json = await summaryRes.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(json);

                    model.Summary.TotalStudents = data.totalStudents;
                    model.Summary.TotalTeachers = data.totalActiveTeachers;
                    model.Summary.ActiveCourses = data.activeCoursesThisSemester;
                    model.Summary.CurrentAcademicYear = "2024-2025";
                    model.Summary.CurrentSemester = "Fall";

                }

                var deptRes = await _client.GetAsync($"{_apiBase}Departments");
                if (deptRes.IsSuccessStatusCode)
                {
                    var deptJson = await deptRes.Content.ReadAsStringAsync();
                    var departments = JsonConvert.DeserializeObject<List<DepartmentDashboardView>>(deptJson);
                    model.Departments = departments;
                    model.Summary.Departments = departments?.Count ?? 0;
                }

                var pendingRes = await _client.GetAsync($"{_apiBase}AdminGrades/pending");
                if (pendingRes.IsSuccessStatusCode)
                {
                    var pendingJson = await pendingRes.Content.ReadAsStringAsync();
                    model.PendingGrades =
                        JsonConvert.DeserializeObject<List<AdminPendingGradeView>>(pendingJson) ?? new();

                    model.Summary.PendingGrades = model.PendingGrades.Count;
                }

            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View("~/Views/Dashboard/Admin.cshtml", model);
        }

        // =======================
        // VALIDATE ALL
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateAllGrades()
        {
            try
            {
                var pendingRes = await _client.GetAsync($"{_apiBase}AdminGrades/pending");
                if (!pendingRes.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to load pending grades.";
                    return RedirectToAction("Dashboard");
                }

                var json = await pendingRes.Content.ReadAsStringAsync();
                var grades = JsonConvert.DeserializeObject<List<AdminPendingGradeView>>(json) ?? new();

                int success = 0;

                foreach (var g in grades)
                {
                    var res = await _client.PostAsync(
                        $"{_apiBase}AdminGrades/validate/{g.TestGradeID}", null);

                    if (res.IsSuccessStatusCode)
                        success++;
                }

                TempData["Success"] =
                    success == grades.Count
                        ? "All grades validated successfully."
                        : $"Validated {success} grades. Some failed.";

                return RedirectToAction("Dashboard");
            }
            catch
            {
                TempData["Error"] = "Failed to validate all grades.";
                return RedirectToAction("Dashboard");
            }
        }

        // =======================
        // REJECT ALL
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAllGrades()
        {
            try
            {
                var pendingRes = await _client.GetAsync($"{_apiBase}AdminGrades/pending");
                if (!pendingRes.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Failed to load pending grades.";
                    return RedirectToAction("Dashboard");
                }

                var json = await pendingRes.Content.ReadAsStringAsync();
                var grades = JsonConvert.DeserializeObject<List<AdminPendingGradeView>>(json) ?? new();

                int success = 0;

                foreach (var g in grades)
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Delete,
                        $"{_apiBase}AdminGrades/reject/{g.TestGradeID}");

                    var res = await _client.SendAsync(request);

                    if (res.IsSuccessStatusCode)
                        success++;
                }

                TempData["Success"] =
                    success == grades.Count
                        ? "All grades rejected successfully."
                        : $"Rejected {success} grades. Some failed.";

                return RedirectToAction("Dashboard");
            }
            catch
            {
                TempData["Error"] = "Failed to reject all grades.";
                return RedirectToAction("Dashboard");
            }
        }
    }
}
