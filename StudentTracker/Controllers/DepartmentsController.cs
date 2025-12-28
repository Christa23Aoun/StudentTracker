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

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public DepartmentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient Api() => _httpClientFactory.CreateClient("API");

        public async Task<IActionResult> Index()
        {
            var res = await Api().GetAsync("departments");
            var list = new List<DepartmentView>();

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadAsStringAsync();
                list = JsonSerializer.Deserialize<List<DepartmentView>>(json, _json) ?? new();
                list = list.OrderByDescending(d => d.DepartmentID).ToList();
            }

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var res = await Api().GetAsync($"departments/{id}");
            if (!res.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var department = JsonSerializer.Deserialize<DepartmentView>(json, _json);

            return department == null
                ? RedirectToAction(nameof(Index))
                : View(department);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentView model)
        {
            if (!ModelState.IsValid) return View(model);

            var payload = new StringContent(
                JsonSerializer.Serialize(model, _json),
                Encoding.UTF8,
                "application/json");

            var res = await Api().PostAsync("departments", payload);

            TempData["Msg"] = res.IsSuccessStatusCode
                ? "✅ Department created successfully."
                : "❌ Failed to create department.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var res = await Api().GetAsync($"departments/{id}");
            if (!res.IsSuccessStatusCode) return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<DepartmentView>(json, _json);

            return data == null
                ? RedirectToAction(nameof(Index))
                : View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DepartmentView model)
        {
            if (id != model.DepartmentID || !ModelState.IsValid)
                return View(model);

            var payload = new StringContent(
                JsonSerializer.Serialize(model, _json),
                Encoding.UTF8,
                "application/json");

            var res = await Api().PutAsync($"departments/{id}", payload);

            TempData["Msg"] = res.IsSuccessStatusCode
                ? "✅ Department updated successfully."
                : "❌ Failed to update department.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int departmentId)
        {
            var client = Api();

            var res = await client.GetAsync($"departments/{departmentId}");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Msg"] = "⚠️ Department not found.";
                return RedirectToAction(nameof(Index));
            }

            var json = await res.Content.ReadAsStringAsync();
            var dep = JsonSerializer.Deserialize<DepartmentView>(json, _json);

            if (dep == null)
            {
                TempData["Msg"] = "⚠️ Department not found.";
                return RedirectToAction(nameof(Index));
            }

            if (!dep.IsActive)
            {
                TempData["Msg"] = "ℹ️ This department is already inactive.";
                return RedirectToAction(nameof(Index));
            }

            dep.IsActive = false;

            var payload = new StringContent(
                JsonSerializer.Serialize(dep, _json),
                Encoding.UTF8,
                "application/json");

            var update = await client.PutAsync($"departments/{departmentId}", payload);

            TempData["Msg"] = update.IsSuccessStatusCode
                ? "✅ Department deactivated successfully."
                : "❌ Failed to deactivate department.";

            return RedirectToAction(nameof(Index));
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int departmentId)
        {
            var res = await Api().PutAsync($"departments/{departmentId}/reactivate", null);

            TempData["Msg"] = res.IsSuccessStatusCode
                ? "✅ Department reactivated successfully."
                : "ℹ️ Department is already active or cannot be reactivated.";

            return RedirectToAction(nameof(Index));
        }
    }
}
