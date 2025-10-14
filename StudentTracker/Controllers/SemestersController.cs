using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers;

public class SemestersController : Controller
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
        if (!res.IsSuccessStatusCode) return new List<AcademicYearOption>();
        var json = await res.Content.ReadAsStringAsync();
        var list = JsonConvert.DeserializeObject<List<AcademicYearOption>>(json) ?? new();
        return list;
    }

    public async Task<IActionResult> Index()
    {
        var res = await _client.GetAsync($"{_apiBase}Semesters");
        if (!res.IsSuccessStatusCode) return View(new List<SemesterView>());
        var json = await res.Content.ReadAsStringAsync();
        var list = JsonConvert.DeserializeObject<List<SemesterView>>(json) ?? new();
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        return View(new SemesterView { AcademicYears = await LoadAcademicYearsAsync() });
    }

    [HttpPost]
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

        var res = await _client.PostAsync($"{_apiBase}Semesters", new StringContent(payload, Encoding.UTF8, "application/json"));
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Create failed.");
            model.AcademicYears = await LoadAcademicYearsAsync();
            return View(model);
        }

        TempData["Msg"] = "Semester created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var res = await _client.GetAsync($"{_apiBase}Semesters/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var json = await res.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<SemesterView>(json)!;
        item.AcademicYears = await LoadAcademicYearsAsync();
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, SemesterView model)
    {
        if (id != model.SemesterID) return BadRequest();
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

        var res = await _client.PutAsync($"{_apiBase}Semesters/{id}", new StringContent(payload, Encoding.UTF8, "application/json"));
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Update failed.");
            model.AcademicYears = await LoadAcademicYearsAsync();
            return View(model);
        }

        TempData["Msg"] = "Semester updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var res = await _client.GetAsync($"{_apiBase}Semesters/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var json = await res.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<SemesterView>(json);
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var res = await _client.DeleteAsync($"{_apiBase}Semesters/{id}");
        TempData["Msg"] = res.IsSuccessStatusCode ? "Semester deleted." : "Delete failed.";
        return RedirectToAction(nameof(Index));
    }
}
