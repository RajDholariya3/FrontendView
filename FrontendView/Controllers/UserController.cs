using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Model;
using System.Net.Http.Headers;

namespace FrontendView.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;

        public UserController(IHttpContextAccessor httpContextAccessor)
        {
            _baseAddress = new Uri("http://localhost:5012/api");
            _httpClient = new HttpClient { BaseAddress = _baseAddress };

            string token = httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UserList()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Login");
            }

            UserModel user = null;

            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Users/{userId}");
            string data = await response.Content.ReadAsStringAsync();

            Console.WriteLine("API Response: " + data);

            if (response.IsSuccessStatusCode)
            {
                user = JsonConvert.DeserializeObject<UserModel>(data);
            }

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("ErrorPage"); // Redirect to an error page if needed
            }

            return View(user); // Pass a single user object
        }


        [HttpGet]
        public async Task<IActionResult> AddUser(int? userId)
        {
            if (userId.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<UserModel>(data);
                    return View(user);
                }
            }
            return View(new UserModel());
        }

        [HttpPost]
        public async Task<IActionResult> Save(UserModel user)
        {
            if (ModelState.IsValid)
            {
                user.UserId = user.UserId > 0 ? user.UserId : 0;
                var json = JsonConvert.SerializeObject(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (user.UserId == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Users", content);
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Users/{user.UserId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("UserList");
                }

                ViewBag.ErrorMessage = "Error saving user.";
            }
            return View("AddUser", user);
        }

        public async Task<IActionResult> Delete(int userId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Users/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "User deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error deleting user: {response.StatusCode} - {response.ReasonPhrase}\nDetails: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the user: {ex.Message}";
            }
            return RedirectToAction("UserList");
        }
    }
}
