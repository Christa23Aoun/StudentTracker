using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // Only authorized users can access attendance pages
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public AttendanceController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // GET: /Attendance
        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}Attendance");
            if (!res.IsSuccessStatusCode)
                return View(new List<AttendanceView>());

            var json = await res.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<AttendanceView>>(json) ?? new();
            return View(data);
        }

        // GET: /Attendance/Create
        public IActionResult Create() => View();

        // POST: /Attendance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AttendanceView model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payload = JsonConvert.SerializeObject(model);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}Attendance", content);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to add attendance record.");
                return View(model);
            }

            TempData["Msg"] = "Attendance record created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Attendance/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Attendance/{id}");
            if (!res.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<AttendanceView>(json);
            if (item == null)
                return RedirectToAction(nameof(Index));

            return View(item);
        }

        // POST: /Attendance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = await _client.DeleteAsync($"{_apiBase}Attendance/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "Attendance record deleted successfully."
                : "Failed to delete attendance record.";

            return RedirectToAction(nameof(Index));
        }
    }
}
