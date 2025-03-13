using FrontendView.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // ⭐ Added for .Any() in Wishlist action
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static FrontendView.Models.DropDownModel;

namespace FrontendView.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;

        public ProductController(IHttpContextAccessor httpContextAccessor)
        {
            _baseAddress = new Uri("http://localhost:5012/api");
            _httpClient = new HttpClient
            {
                BaseAddress = _baseAddress
            };
            string token = httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProductList()
        {
            List<ProductModel> productList = new List<ProductModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Products");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                productList = JsonConvert.DeserializeObject<List<ProductModel>>(data);
            }
            List<CategoriesModel> categoriesList = new List<CategoriesModel>();
            HttpResponseMessage catResponse = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Categories");
            if (catResponse.IsSuccessStatusCode)
            {
                string catData = await catResponse.Content.ReadAsStringAsync();
                categoriesList = JsonConvert.DeserializeObject<List<CategoriesModel>>(catData);
            }
            ViewBag.Categories = categoriesList;

            var wishlist = GetWishlistFromSession();
            ViewBag.Wishlist = wishlist;
            return View(productList);
        }

        // ⭐ New Action: Wishlist
        [HttpGet]
        public async Task<IActionResult> Wishlist()
        {
            var wishlistIds = GetWishlistFromSession(); // Get wishlist product IDs from session
            List<ProductModel> wishlistProducts = new List<ProductModel>();

            if (wishlistIds.Any()) // ⭐ Using .Any() to check if there are items
            {
                // Fetch product details for each ID from the product API
                foreach (var productId in wishlistIds)
                {
                    HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Products/{productId}");
                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        var product = JsonConvert.DeserializeObject<ProductModel>(data);
                        if (product != null)
                        {
                            wishlistProducts.Add(product);
                        }
                    }
                    else
                    {
                        // Log missing products (optional cleanup could be added here)
                        Console.WriteLine($"Product {productId} not found in API.");
                    }
                }
            }

            return View(wishlistProducts); // Pass the list of products to the view
        }

        [HttpPost]
        public IActionResult ToggleWishlist([FromBody] WishlistRequest request)
        {
            try
            {
                if (request == null || request.ProductId <= 0)
                {
                    return Json(new { success = false, message = "Invalid product ID" });
                }

                var wishlist = GetWishlistFromSession();
                if (wishlist.Contains(request.ProductId))
                {
                    wishlist.Remove(request.ProductId); // Remove from wishlist
                }
                else
                {
                    wishlist.Add(request.ProductId); // Add to wishlist
                }

                SaveWishlistToSession(wishlist);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ⭐ Helper method to get wishlist from session (unchanged)
        private List<int> GetWishlistFromSession()
        {
            var wishlistJson = HttpContext.Session.GetString("Wishlist");
            return string.IsNullOrEmpty(wishlistJson)
                ? new List<int>()
                : JsonConvert.DeserializeObject<List<int>>(wishlistJson);
        }

        // ⭐ Helper method to save wishlist to session (unchanged)
        private void SaveWishlistToSession(List<int> wishlist)
        {
            HttpContext.Session.SetString("Wishlist", JsonConvert.SerializeObject(wishlist));
        }

        // ⭐ Model for wishlist request (unchanged)
        public class WishlistRequest
        {
            public int ProductId { get; set; }
        }




        [HttpGet]
        public async Task<IActionResult> ProductsByCategory(int categoryId)
        {
            List<ProductModel> productList = new List<ProductModel>();
            try
            {
                string url = categoryId == 0
                    ? $"{_httpClient.BaseAddress}/Products"
                    : $"{_httpClient.BaseAddress}/Products/Category/{categoryId}";

                Console.WriteLine($"Requesting: {url}");
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                Console.WriteLine($"Status Code: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response: {data}");

                    if (!string.IsNullOrEmpty(data))
                    {
                        productList = JsonConvert.DeserializeObject<List<ProductModel>>(data) ?? new List<ProductModel>();
                    }
                    else
                    {
                        Console.WriteLine("Empty response from API");
                    }
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, new { success = false, message = ex.Message });
            }

            return Json(productList);
        }

    }
}