using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using StudentTracker.Models;

namespace StudentTracker.Controllers;

public class DepartmentsController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBase;

    public DepartmentsController(IHttpClientFactory factory, IConfiguration config)
    {
        _httpClient = factory.CreateClient();
        _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
    }

    // GET: Departments
    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync($"{_apiBase}Departments");
        if (!response.IsSuccessStatusCode)
            return View(new List<dynamic>());

        var json = await response.Content.ReadAsStringAsync();
        var list = JsonConvert.DeserializeObject<List<DepartmentView>>(json);
        return View(list);
    }
    // GET: Departments/Create
    public IActionResult Create() => View();

    // POST: Departments/Create
    [HttpPost]
    public async Task<IActionResult> Create(DepartmentView model)
    {
        if (!ModelState.IsValid) return View(model);

        var payload = JsonConvert.SerializeObject(new { departmentName = model.DepartmentName });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_apiBase}Departments", content);
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Failed to create department.");
            return View(model);
        }

        TempData["Msg"] = "Department created successfully.";
        return RedirectToAction(nameof(Index));
    }
    // GET: Departments/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var res = await _httpClient.GetAsync($"{_apiBase}Departments/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();

        var json = await res.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<DepartmentView>(json);
        return View(item);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, DepartmentView model)
    {
        if (id != model.DepartmentID) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var payload = JsonConvert.SerializeObject(new { departmentID = model.DepartmentID, departmentName = model.DepartmentName });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var res = await _httpClient.PutAsync($"{_apiBase}Departments/{id}", content);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Failed to update department.");
            return View(model);
        }

        TempData["Msg"] = "Department updated successfully.";
        return RedirectToAction(nameof(Index));
    }
    // GET: Departments/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _httpClient.GetAsync($"{_apiBase}Departments/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();

        var json = await res.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<DepartmentView>(json);
        return View(item);
    }

    // POST: Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var res = await _httpClient.DeleteAsync($"{_apiBase}Departments/{id}");
        if (!res.IsSuccessStatusCode)
        {
            TempData["Msg"] = "Delete failed.";
            return RedirectToAction(nameof(Index));
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
