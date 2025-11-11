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

        // ✅ Fixed: no "Web" enum here
        private static readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public DepartmentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient Api() => _httpClientFactory.CreateClient("API");

        // ===== Index =====
        public async Task<IActionResult> Index()
        {
            var client = Api();
            var res = await client.GetAsync("departments");

            var list = new List<DepartmentView>();

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                list = JsonSerializer.Deserialize<List<DepartmentView>>(json, _json) ?? new();
                list = list.OrderByDescending(d => d.DepartmentID).ToList();
            }
            else
            {
                ViewBag.Error = $"⚠️ Unable to fetch departments (HTTP {(int)res.StatusCode}).";
            }

            return View(list);
        }

        // ===== Create =====
        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = Api();
            var payload = new StringContent(
                JsonSerializer.Serialize(model, _json),
                Encoding.UTF8,
                "application/json");

            var res = await client.PostAsync("departments", payload);

            if (res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "✅ Department created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "❌ Failed to create department.";
            return View(model);
        }

        // ===== Edit =====
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = Api();
            var res = await client.GetAsync($"departments/{id}");

            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<DepartmentView>(json, _json);
            if (data == null)
                return RedirectToAction(nameof(Index));

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentView model)
        {
            if (id != model.DepartmentID) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var client = Api();
            var body = JsonSerializer.Serialize(model, _json);
            var payload = new StringContent(body, Encoding.UTF8, "application/json");

            var res = await client.PutAsync($"departments/{id}", payload);

            if (!res.IsSuccessStatusCode)
            {
                ViewBag.Error = "❌ Failed to update department.";
                return View(model);
            }

            TempData["Msg"] = "✅ Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ===== Delete =====
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var client = Api();
            var res = await client.GetAsync($"departments/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<DepartmentView>(json, _json);
            if (data == null)
                return RedirectToAction(nameof(Index));

            return View(data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int departmentId)
        {
            var client = Api();
            var res = await client.DeleteAsync($"departments/{departmentId}");

            TempData["Msg"] = res.IsSuccessStatusCode
                ? "✅ Department deleted successfully."
                : "⚠️ Failed to delete department.";

            return RedirectToAction(nameof(Index));
        }

        // ===== Details (Info Page) =====
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = Api();
            var res = await client.GetAsync($"departments/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var department = JsonSerializer.Deserialize<DepartmentView>(json, _json);
            if (department == null)
                return RedirectToAction(nameof(Index));

            return View(department);
        }
    }
}
