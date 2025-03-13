using System.Security.Claims;
using System.Text;
using FrontendView.Models;
using FrontendView.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FrontendView.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "http://localhost:5012/api"; // API Base URL

        public LoginController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid login data." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var endpoint = $"{_apiBaseUrl}/Login";
                var jsonData = JsonConvert.SerializeObject(user);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var loggedInUser = JsonConvert.DeserializeObject<LoginResponseModel>(responseData);

                    if (loggedInUser != null)
                    {
                        // Store authentication details in session
                        HttpContext.Session.SetString("Token", loggedInUser.Token);
                        HttpContext.Session.SetString("UserId", loggedInUser.UserId.ToString());
                        HttpContext.Session.SetString("UserName", loggedInUser.Username);
                        /*                        HttpContext.Session.SetString("Role", loggedInUser.IsAdmin.ToString());
                        */
                        Console.WriteLine($"Login Successful: Stored UserId = {loggedInUser.UserId}");



                        return RedirectToAction("Index", "Home");
                    }
                }

                return Unauthorized(new { message = "Invalid username or password." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred during login.", error = ex.Message });
            }
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserModel user)
        {
            user.UserId ??= 0;
            user.IsAdmin = false;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var endpoint = $"{_apiBaseUrl}/Users";
                var jsonData = JsonConvert.SerializeObject(user);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);
                Console.WriteLine(response);

                if (response.IsSuccessStatusCode)
                {
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("UserName", user.Name);
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    return RedirectToAction("Index", "Home");
                }

                return View("SignUp");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error occurred during user registration.", error = ex.Message });
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
