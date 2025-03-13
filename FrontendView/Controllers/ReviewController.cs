using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Model;
using static FrontendView.Models.DropDownModel;
using System.Net.Http.Headers;

namespace FrontendView.Controllers
{
    public class ReviewController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;

        public ReviewController(IHttpContextAccessor httpContextAccessor)
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
        public async Task<IActionResult> ReviewList()
        {
            List<ReviewModel> reviewList = new List<ReviewModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Reviews");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                reviewList = JsonConvert.DeserializeObject<List<ReviewModel>>(data);
            }
            return View(reviewList);
        }

        [HttpGet]
        public async Task<IActionResult> AddReview(int? reviewId)
        {
            await LoadProductDropdownList();
            await LoadUserDropdownList();

            if (reviewId.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Reviews/{reviewId.Value}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var review = JsonConvert.DeserializeObject<ReviewModel>(data);
                    return View(review);
                }
            }
            return View(new ReviewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Save(ReviewModel review)
        {
            if (ModelState.IsValid)
            {
                review.ReviewId = review.ReviewId > 0 ? review.ReviewId : 0;
                var json = JsonConvert.SerializeObject(review);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (review.ReviewId == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Reviews", content);
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Reviews/{review.ReviewId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ReviewList");
                }

                ViewBag.ErrorMessage = "Error saving review.";
            }
            await LoadProductDropdownList();
            await LoadUserDropdownList();
            return View("AddReview", review);
        }

        public async Task<IActionResult> Delete(int reviewId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Reviews/{reviewId}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Review deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error deleting review: {response.StatusCode} - {response.ReasonPhrase}\nDetails: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the review: {ex.Message}";
            }
            return RedirectToAction("ReviewList");
        }

        private async Task LoadUserDropdownList()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Users/Drop-Down");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserDropDownModel>>(data);
                ViewBag.UserList = users;
            }
        }

        private async Task LoadProductDropdownList()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Products/Drop-Down");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<ProductDropdownModel>>(data);
                ViewBag.ProductList = products;
            }
        }
    }
}
