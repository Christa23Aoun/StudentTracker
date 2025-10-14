using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers;

public class AcademicYearsController : Controller
{
    private readonly HttpClient _client;
    private readonly string _apiBase;

    public AcademicYearsController(IHttpClientFactory factory, IConfiguration config)
    {
        _client = factory.CreateClient();
        _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
    }

    public async Task<IActionResult> Index()
    {
        var res = await _client.GetAsync($"{_apiBase}AcademicYears");
        if (!res.IsSuccessStatusCode) return View(new List<AcademicYearView>());
        var json = await res.Content.ReadAsStringAsync();
        var list = JsonConvert.DeserializeObject<List<AcademicYearView>>(json) ?? new();
        return View(list);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(AcademicYearView model)
    {
        if (!ModelState.IsValid) return View(model);
        var payload = JsonConvert.SerializeObject(new { name = model.Name, startDate = model.StartDate, endDate = model.EndDate });
        var res = await _client.PostAsync($"{_apiBase}AcademicYears", new StringContent(payload, Encoding.UTF8, "application/json"));
        if (!res.IsSuccessStatusCode) { ModelState.AddModelError("", "Create failed."); return View(model); }
        TempData["Msg"] = "Academic year created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var res = await _client.GetAsync($"{_apiBase}AcademicYears/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var json = await res.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<AcademicYearView>(json);
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AcademicYearView model)
    {
        if (id != model.AcademicYearID) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var payload = JsonConvert.SerializeObject(new { academicYearID = model.AcademicYearID, name = model.Name, startDate = model.StartDate, endDate = model.EndDate });
        var res = await _client.PutAsync($"{_apiBase}AcademicYears/{id}", new StringContent(payload, Encoding.UTF8, "application/json"));
        if (!res.IsSuccessStatusCode) { ModelState.AddModelError("", "Update failed."); return View(model); }
        TempData["Msg"] = "Academic year updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var res = await _client.GetAsync($"{_apiBase}AcademicYears/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var json = await res.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<AcademicYearView>(json);
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var res = await _client.DeleteAsync($"{_apiBase}AcademicYears/{id}");
        TempData["Msg"] = res.IsSuccessStatusCode ? "Academic year deleted." : "Delete failed.";
        return RedirectToAction(nameof(Index));
    }
}
