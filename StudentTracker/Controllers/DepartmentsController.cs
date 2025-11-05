using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Models;
using System.Text;
using System.Text.Json;

namespace StudentTracker.Controllers
{
    [Authorize]
    public class DepartmentsController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Make JSON tolerant to Pascal/camel casing
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public DepartmentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient Api() => _httpClientFactory.CreateClient("API"); // BaseAddress = https://localhost:7199/api/

        // GET: /Departments
        public async Task<IActionResult> Index()
        {
            var client = Api();
            var res = await client.GetAsync("departments");  // ✅ not api/departments

            var list = new List<DepartmentView>();

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                list = JsonSerializer.Deserialize<List<DepartmentView>>(json, _json) ?? new();
                // Optional: newest first
                list = list.OrderByDescending(d => d.DepartmentID).ToList();
            }
            else
            {
                ViewBag.Error = $"⚠️ Unable to fetch departments (HTTP {(int)res.StatusCode}).";
            }

            return View(list);
        }

        // GET: /Departments/Create
        public IActionResult Create() => View();

        // POST: /Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentView model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = Api();

            // The API expects DepartmentCreateDto { DepartmentName }
            var body = JsonSerializer.Serialize(new { departmentName = model.DepartmentName }, _json);
            var payload = new StringContent(body, Encoding.UTF8, "application/json");

            var res = await client.PostAsync("departments", payload); // ✅ not api/departments

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                ViewBag.Error = $"❌ Failed to create department. {(int)res.StatusCode} {res.ReasonPhrase}";
                return View(model);
            }

            TempData["Msg"] = "✅ Department created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = Api();
            var res = await client.GetAsync($"departments/{id}"); // ✅

            if (!res.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<DepartmentView>(json, _json);
            if (data == null) return RedirectToAction(nameof(Index));

            return View(data);
        }

        // POST: /Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentView model)
        {
            if (id != model.DepartmentID) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var client = Api();
            var body = JsonSerializer.Serialize(new { departmentID = model.DepartmentID, departmentName = model.DepartmentName }, _json);
            var payload = new StringContent(body, Encoding.UTF8, "application/json");

            var res = await client.PutAsync($"departments/{id}", payload); // ✅

            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Error = "❌ Failed to update department.";
                return View(model);
            }

            TempData["Msg"] = "✅ Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = Api();
            var res = await client.GetAsync($"departments/{id}"); // ✅
            if (!res.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<DepartmentView>(json, _json);
            if (data == null) return RedirectToAction(nameof(Index));

            return View(data);
        }

        // POST: /Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int departmentId)
        {
            var client = Api();
            var res = await client.DeleteAsync($"departments/{departmentId}"); // ✅

            TempData["Msg"] = res.IsSuccessStatusCode
                ? "✅ Department deleted successfully."
                : "⚠️ Failed to delete department.";

            return RedirectToAction(nameof(Index));
        }
    }
}
