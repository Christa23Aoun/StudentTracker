using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudentTracker.Models;
using System.Text;

namespace StudentTracker.Controllers;

public class CoursesController : Controller
{
    private readonly HttpClient _client;
    private readonly string _apiBase;

    public CoursesController(IHttpClientFactory factory, IConfiguration config)
    {
        _client = factory.CreateClient();
        _apiBase = config.GetSection("ApiSettings:BaseUrl").Value!;
    }

    private async Task<List<LookupItem>> LoadDepartmentsAsync()
    {
        var res = await _client.GetAsync($"{_apiBase}Departments");
        if (!res.IsSuccessStatusCode) return new();
        var json = await res.Content.ReadAsStringAsync();
        // Departments API returns DepartmentID, DepartmentName
        var anon = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new();
        return anon.Select(a => new LookupItem { Id = (int)a.departmentID, Name = (string)a.departmentName }).ToList();
    }

    private async Task<List<LookupItem>> LoadSemestersAsync()
    {
        var res = await _client.GetAsync($"{_apiBase}Semesters");
        if (!res.IsSuccessStatusCode) return new();
        var json = await res.Content.ReadAsStringAsync();
        // Semesters API returns SemesterID, Name
        var anon = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new();
        return anon.Select(a => new LookupItem { Id = (int)a.semesterID, Name = (string)a.name }).ToList();
    }

    public async Task<IActionResult> Index()
    {
        var res = await _client.GetAsync($"{_apiBase}Courses");
        if (!res.IsSuccessStatusCode) return View(new List<CourseView>());
        var json = await res.Content.ReadAsStringAsync();
        var list = JsonConvert.DeserializeObject<List<CourseView>>(json) ?? new();
        return View(list);
    }

    public async Task<IActionResult> Create()
    {
        return View(new CourseView
        {
            Departments = await LoadDepartmentsAsync(),
            Semesters = await LoadSemestersAsync()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CourseView model)
    {
        if (!ModelState.IsValid)
        {
            model.Departments = await LoadDepartmentsAsync();
            model.Semesters = await LoadSemestersAsync();
            return View(model);
        }

        var payload = JsonConvert.SerializeObject(new
        {
            courseCode = model.CourseCode,
            courseName = model.CourseName,
            creditHours = model.CreditHours,
            departmentID = model.DepartmentID,
            teacherID = model.TeacherID,  // numeric for now
            semesterID = model.SemesterID,
            isActive = model.IsActive
        });

        var res = await _client.PostAsync($"{_apiBase}Courses", new StringContent(payload, Encoding.UTF8, "application/json"));
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Create failed.");
            model.Departments = await LoadDepartmentsAsync();
            model.Semesters = await LoadSemestersAsync();
            return View(model);
        }

        TempData["Msg"] = "Course created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var res = await _client.GetAsync($"{_apiBase}Courses/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var json = await res.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<CourseView>(json)!;

        item.Departments = await LoadDepartmentsAsync();
        item.Semesters = await LoadSemestersAsync();

        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, CourseView model)
    {
        if (id != model.CourseID) return BadRequest();
        if (!ModelState.IsValid)
        {
            model.Departments = await LoadDepartmentsAsync();
            model.Semesters = await LoadSemestersAsync();
            return View(model);
        }

        var payload = JsonConvert.SerializeObject(new
        {
            courseID = model.CourseID,
            courseCode = model.CourseCode,
            courseName = model.CourseName,
            creditHours = model.CreditHours,
            departmentID = model.DepartmentID,
            teacherID = model.TeacherID,
            semesterID = model.SemesterID,
            isActive = model.IsActive
        });

        var res = await _client.PutAsync($"{_apiBase}Courses/{id}", new StringContent(payload, Encoding.UTF8, "application/json"));
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Update failed.");
            model.Departments = await LoadDepartmentsAsync();
            model.Semesters = await LoadSemestersAsync();
            return View(model);
        }

        TempData["Msg"] = "Course updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var res = await _client.GetAsync($"{_apiBase}Courses/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var json = await res.Content.ReadAsStringAsync();
        var item = JsonConvert.DeserializeObject<CourseView>(json);
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var res = await _client.DeleteAsync($"{_apiBase}Courses/{id}");
        TempData["Msg"] = res.IsSuccessStatusCode ? "Course deleted." : "Delete failed.";
        return RedirectToAction(nameof(Index));
    }
}
