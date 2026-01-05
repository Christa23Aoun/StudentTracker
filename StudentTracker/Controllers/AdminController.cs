using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
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

        [HttpGet("/admin/reports")]
        public IActionResult Reports()
        {
            return View("~/Views/Dashboard/Reports.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> ReportAbsences()
        {
            var res = await _client.GetAsync($"{_apiBase}AdminReports/excessive-absences");

            if (!res.IsSuccessStatusCode)
                return View("~/Views/Dashboard/ReportAbsences.cshtml", new List<AdminExcessiveAbsenceView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<AdminExcessiveAbsenceView>>(json) ?? new();

            return View("~/Views/Dashboard/ReportAbsences.cshtml", data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAbsencesPdf()
        {
            var res = await _client.GetAsync($"{_apiBase}AdminReports/excessive-absences");
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<AdminExcessiveAbsenceView>>(json) ?? new();

            return new ViewAsPdf("~/Views/Dashboard/ReportAbsences.cshtml", data)
            {
                FileName = "Excessive_Absences_Report.pdf",
                CustomSwitches = "--print-media-type"

            };
        }

        [HttpGet]
        public async Task<IActionResult> ReportFailing()
        {
            var res = await _client.GetAsync($"{_apiBase}AdminReports/failing-students");

            if (!res.IsSuccessStatusCode)
                return View("~/Views/Dashboard/ReportFailing.cshtml", new List<AdminFailingStudentView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<AdminFailingStudentView>>(json) ?? new();

            return View("~/Views/Dashboard/ReportFailing.cshtml", data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportFailingPdf()
        {
            var res = await _client.GetAsync($"{_apiBase}AdminReports/failing-students");
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<AdminFailingStudentView>>(json) ?? new();

            return new ViewAsPdf("~/Views/Dashboard/ReportFailing.cshtml", data)
            {
                FileName = "Failing_Students_Report.pdf",
                CustomSwitches = "--print-media-type"

            };
        }

        [HttpGet]
        public async Task<IActionResult> ReportExcellent()
        {
            var res = await _client.GetAsync($"{_apiBase}AdminReports/excellent-students");

            if (!res.IsSuccessStatusCode)
                return View("~/Views/Dashboard/ReportExcellent.cshtml", new List<AdminExcellentStudentView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<AdminExcellentStudentView>>(json) ?? new();

            return View("~/Views/Dashboard/ReportExcellent.cshtml", data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcellentPdf()
        {
            var res = await _client.GetAsync($"{_apiBase}AdminReports/excellent-students");
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<AdminExcellentStudentView>>(json) ?? new();

            return new ViewAsPdf("~/Views/Dashboard/ReportExcellent.cshtml", data)
            {
                FileName = "Excellent_Students_Report.pdf",
                CustomSwitches = "--print-media-type"

            };
        }
    }
}
