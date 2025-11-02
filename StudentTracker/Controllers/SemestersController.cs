using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers
{
    // 🔒 Only authorized (logged-in) users can manage semesters
    [Authorize]
    public class SemestersController : BaseController
    {
        private readonly HttpClient _client;
        private readonly string _apiBase;

        public SemestersController(IHttpClientFactory factory, IConfiguration config)
        {
            _client = factory.CreateClient();
            _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
        }

        // Helper: fetch academic years for dropdown
        private async Task<List<AcademicYearOption>> LoadAcademicYearsAsync()
        {
            var res = await _client.GetAsync($"{_apiBase}AcademicYears");
            if (!res.IsSuccessStatusCode)
                return new List<AcademicYearOption>();

            var json = await res.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<AcademicYearOption>>(json) ?? new();
            return list;
        }

        // GET: /Semesters
        public async Task<IActionResult> Index()
        {
            var res = await _client.GetAsync($"{_apiBase}Semesters");
            if (!res.IsSuccessStatusCode)
                return View(new List<SemesterView>());

            var json = await res.Content.ReadAsStringAsync();
            var list = JsonConvert.DeserializeObject<List<SemesterView>>(json) ?? new();
            return View(list);
        }

        // GET: /Semesters/Create
        public async Task<IActionResult> Create()
        {
            var model = new SemesterView
            {
                AcademicYears = await LoadAcademicYearsAsync()
            };
            return View(model);
        }

        // POST: /Semesters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SemesterView model)
        {
            if (!ModelState.IsValid)
            {
                model.AcademicYears = await LoadAcademicYearsAsync();
                return View(model);
            }

            var payload = JsonConvert.SerializeObject(new
            {
                academicYearID = model.AcademicYearID,
                name = model.Name,
                semesterNb = model.SemesterNb,
                startDate = model.StartDate,
                endDate = model.EndDate
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PostAsync($"{_apiBase}Semesters", content);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create semester.");
                model.AcademicYears = await LoadAcademicYearsAsync();
                return View(model);
            }

            TempData["Msg"] = "Semester created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Semesters/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Semesters/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<SemesterView>(json);
            if (item == null)
                return NotFound();

            item.AcademicYears = await LoadAcademicYearsAsync();
            return View(item);
        }

        // POST: /Semesters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SemesterView model)
        {
            if (id != model.SemesterID)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                model.AcademicYears = await LoadAcademicYearsAsync();
                return View(model);
            }

            var payload = JsonConvert.SerializeObject(new
            {
                semesterID = model.SemesterID,
                academicYearID = model.AcademicYearID,
                name = model.Name,
                semesterNb = model.SemesterNb,
                startDate = model.StartDate,
                endDate = model.EndDate
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _client.PutAsync($"{_apiBase}Semesters/{id}", content);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to update semester.");
                model.AcademicYears = await LoadAcademicYearsAsync();
                return View(model);
            }

            TempData["Msg"] = "Semester updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Semesters/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _client.GetAsync($"{_apiBase}Semesters/{id}");
            if (!res.IsSuccessStatusCode)
                return NotFound();

            var json = await res.Content.ReadAsStringAsync();
            var item = JsonConvert.DeserializeObject<SemesterView>(json);
            if (item == null)
                return NotFound();

            return View(item);
        }

        // POST: /Semesters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var res = await _client.DeleteAsync($"{_apiBase}Semesters/{id}");
            TempData["Msg"] = res.IsSuccessStatusCode
                ? "Semester deleted successfully."
                : "Failed to delete semester.";
            return RedirectToAction(nameof(Index));
        }
    }
}
