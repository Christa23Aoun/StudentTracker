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
                }

                var deptRes = await _client.GetAsync($"{_apiBase}Departments");
                if (deptRes.IsSuccessStatusCode)
                {
                    var deptJson = await deptRes.Content.ReadAsStringAsync();
                    var departments = JsonConvert.DeserializeObject<List<DepartmentDashboardView>>(deptJson);

                    if (departments != null)
                    {
                        model.Departments = departments;
                        model.Summary.Departments = departments.Count;
                    }
                }

                var pendingRes = await _client.GetAsync($"{_apiBase}Dashboard/PendingGrades");
                if (pendingRes.IsSuccessStatusCode)
                {
                    var pendingJson = await pendingRes.Content.ReadAsStringAsync();
                    var pendingList = JsonConvert.DeserializeObject<List<AdminPendingGradeView>>(pendingJson);

                    if (pendingList != null)
                        model.PendingGrades = pendingList;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Server error: {ex.Message}";
            }

            return View("~/Views/Dashboard/Admin.cshtml", model);
        }
    }
}
