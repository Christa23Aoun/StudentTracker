using Microsoft.AspNetCore.Mvc;
using StudentTrackerCOMMON.Models;
using System.Net.Http.Json;

namespace StudentTracker.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;

        public UsersController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient("API");
        }

        public async Task<IActionResult> Index()
        {
            var users = await _httpClient.GetFromJsonAsync<List<User>>("api/Users");
            return View(users);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Users/create", user);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");
            ViewBag.Error = "Unable to create user.";
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<User>($"api/Users/email/{id}");
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            var response = await _httpClient.PutAsJsonAsync("api/Users/update", user);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");
            ViewBag.Error = "Update failed.";
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Users/{id}");
            return RedirectToAction("Index");
        }
    }
}
