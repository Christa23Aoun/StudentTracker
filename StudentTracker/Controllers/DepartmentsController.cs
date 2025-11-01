using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class DepartmentsController : Controller
    {
        private readonly HttpClient _httpClient;

        public DepartmentsController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("API");
        }

        // ✅ List all
        public async Task<IActionResult> Index()
        {
            var departments = await _httpClient.GetFromJsonAsync<List<Department>>("api/Departments");
            return View(departments ?? new List<Department>());
        }

        // ✅ Create (GET)
        [HttpGet]
        public IActionResult Create() => View();

        // ✅ Create (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department dept)
        {
            if (!ModelState.IsValid) return View(dept);

            var payload = new { DepartmentName = dept.DepartmentName };
            var response = await _httpClient.PostAsJsonAsync("api/Departments", payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ Department created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "❌ Failed to create department.";
            return View(dept);
        }

        // ✅ Edit (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dept = await _httpClient.GetFromJsonAsync<Department>($"api/Departments/{id}");
            if (dept == null) return NotFound();
            return View(dept);
        }

        // ✅ Edit (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department dept)
        {
            if (!ModelState.IsValid) return View(dept);

            // must include id in route and proper DTO fields
            var payload = new
            {
                DepartmentID = dept.DepartmentID,
                DepartmentName = dept.DepartmentName
            };

            var response = await _httpClient.PutAsJsonAsync($"api/Departments/{dept.DepartmentID}", payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ Department updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "❌ Failed to update department.";
            return View(dept);
        }

        // ✅ Delete (GET confirmation)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var dept = await _httpClient.GetFromJsonAsync<Department>($"api/Departments/{id}");
            if (dept == null) return NotFound();
            return View(dept);
        }

        // ✅ Delete (POST confirmed)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Departments/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Msg"] = "🗑️ Department deleted successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Msg"] = "❌ Failed to delete department.";

            return View("index");
        }
        TempData["Msg"] = "Department deleted.";
        return RedirectToAction(nameof(Index));
    }

    // You can later add Create/Edit/Delete that POST/PUT/DELETE to API
}

public class DepartmentView
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
