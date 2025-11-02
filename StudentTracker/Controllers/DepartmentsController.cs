using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentTracker.Models;
using System.Text;
using System.Text.Json;

namespace StudentTracker.Controllers
{
    // Require authentication to access department pages
    [Authorize]
    public class DepartmentsController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public DepartmentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Departments
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("API");
            var res = await client.GetAsync("api/departments");

            if (!res.IsSuccessStatusCode)
                return View(new List<DepartmentView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<DepartmentView>>(json, _json) ?? new();
            return View(data);
        }

        // GET: /Departments/Create
        public IActionResult Create() => View();

        // POST: /Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("API");
            var payload = new StringContent(JsonSerializer.Serialize(model, _json), Encoding.UTF8, "application/json");
            var res = await client.PostAsync("api/departments", payload);

            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to create department.";
                return View(model);
            }

            TempData["Msg"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("API");
            var res = await client.GetAsync($"api/departments/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<DepartmentView>(json, _json);
            if (data == null)
                return RedirectToAction(nameof(Index));

            return View(data);
        }

        // POST: /Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentView model)
        {
            if (id != model.DepartmentID)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("API");
            var payload = new StringContent(JsonSerializer.Serialize(model, _json), Encoding.UTF8, "application/json");
            var res = await client.PutAsync($"api/departments/{id}", payload);

            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to update department.";
                return View(model);
            }

            TempData["Msg"] = "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Departments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("API");
            var res = await client.GetAsync($"api/departments/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<DepartmentView>(json, _json);
            if (data == null)
                return RedirectToAction(nameof(Index));

            return View(data);
        }

        // POST: /Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int departmentId)
        {
            var client = _httpClientFactory.CreateClient("API");
            var res = await client.DeleteAsync($"api/departments/{departmentId}");

            if (!res.IsSuccessStatusCode)
                TempData["Error"] = "Failed to delete department.";
            else
                TempData["Msg"] = "Department deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
