using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Models;
using System.Net.Http.Headers;

namespace FrontendView.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;

        public CategoriesController(IHttpContextAccessor httpContextAccessor)
        {
            _baseAddress = new Uri("http://localhost:5012/api");
            _httpClient = new HttpClient { BaseAddress = _baseAddress };

            // Retrieve token from session and set Authorization header
            string token = httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CategoriesList()
        {
            List<CategoriesModel> categoriesList = new List<CategoriesModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Categories");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                categoriesList = JsonConvert.DeserializeObject<List<CategoriesModel>>(data);
            }
            return View(categoriesList);
        }

        [HttpGet]
        public async Task<IActionResult> AddCategory(int? categoryId)
        {
            if (categoryId.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Categories/{categoryId}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var category = JsonConvert.DeserializeObject<CategoriesModel>(data);
                    return View(category);
                }
            }
            return View(new CategoriesModel());
        }

        [HttpPost]
        public async Task<IActionResult> Save(CategoriesModel category)
        {
            if (ModelState.IsValid)
            {
                category.CategoryId = category.CategoryId > 0 ? category.CategoryId : 0;
                var json = JsonConvert.SerializeObject(category);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (category.CategoryId == 0)
                {
                    // Create new category
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Categories", content);
                }
                else
                {
                    // Update existing category
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Categories/{category.CategoryId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("CategoriesList");
                }

                ViewBag.ErrorMessage = "Error saving category.";
            }
            return View("AddCategory", category);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int categoryId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Categories/{categoryId}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Category deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to delete category. Status: {response.StatusCode} - {response.ReasonPhrase}. Details: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the category: {ex.Message}";
            }

            return RedirectToAction("CategoriesList");
        }
    }
}
