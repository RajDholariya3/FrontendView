using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Models;
using System.Net.Http.Headers;
using static FrontendView.Models.DropDownModel;
using FrontendView.Helper;
using FrontendView.Model; // Add this for OrderModel

namespace FrontendView.Controllers
{
    public class CartController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;
        private readonly MailService _mailService;

        public CartController(IHttpContextAccessor httpContextAccessor, MailService mailService)
        {
            _baseAddress = new Uri("http://localhost:5012/api");
            _httpClient = new HttpClient { BaseAddress = _baseAddress };
            _mailService = mailService;

            string token = httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CartList()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Login");
            }

            List<CartModel> cartList = new List<CartModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAddress}/Cart/{userId}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                if (data.Trim().StartsWith("{"))
                {
                    var singleCartItem = JsonConvert.DeserializeObject<CartModel>(data);
                    cartList = new List<CartModel> { singleCartItem };
                }
                else
                {
                    cartList = JsonConvert.DeserializeObject<List<CartModel>>(data);
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Error fetching cart data.";
            }
            return View(cartList);
        }

        [HttpGet]
        public async Task<IActionResult> AddToCart(int? CartId)
        {
            await LoadProductDropdownList();
            await LoadUserDropdownList();

            if (CartId.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAddress}/Cart/{CartId}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var cartItem = JsonConvert.DeserializeObject<CartModel>(data);
                    return View(cartItem);
                }
            }
            return View(new CartModel());
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CartModel cart)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "Session expired. Please log in again." });
            }

            if (cart == null || cart.ProductId <= 0)
            {
                return Json(new { success = false, message = "Invalid product data." });
            }

            HttpResponseMessage productResponse = await _httpClient.GetAsync($"{_baseAddress}/Products/{cart.ProductId}");
            if (!productResponse.IsSuccessStatusCode)
            {
                return Json(new { success = false, message = "Product details not found." });
            }

            var productJson = await productResponse.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<ProductModel>(productJson);

            if (product == null)
            {
                return Json(new { success = false, message = "Failed to retrieve product details." });
            }

            cart.UserId = userId;
            cart.CartId = 0;
            cart.ProductName = product.Name;
            cart.Price = product.Price;
            cart.ImageUrl = product.ImageUrl;
            cart.CreatedDate = DateTime.UtcNow;
            cart.ModifiedDate = DateTime.UtcNow;

            var json = JsonConvert.SerializeObject(cart);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseAddress}/Cart", content);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Product added to cart!" });
            }
            return Json(new { success = false, message = "Failed to add product." });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int CartId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_baseAddress}/Cart/{CartId}");
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Cart item deleted successfully." });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Error deleting cart item: {response.StatusCode} - {response.ReasonPhrase}\nDetails: {errorContent}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while deleting the cart item: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Login");
            }

            // Fetch cart items
            List<CartModel> cartList = new List<CartModel>();
            HttpResponseMessage cartResponse = await _httpClient.GetAsync($"{_baseAddress}/Cart/{userId}");
            if (!cartResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to fetch cart items.";
                return RedirectToAction("CartList");
            }

            string cartData = await cartResponse.Content.ReadAsStringAsync();
            if (cartData.Trim().StartsWith("{"))
            {
                var singleCartItem = JsonConvert.DeserializeObject<CartModel>(cartData);
                cartList = new List<CartModel> { singleCartItem };
            }
            else
            {
                cartList = JsonConvert.DeserializeObject<List<CartModel>>(cartData);
            }

            if (!cartList.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("CartList");
            }

            // Calculate order details
            decimal grandTotal = cartList.Sum(item => item.Price ?? 0);
            int totalProducts = cartList.Count;

            // Fetch user email and name
            string userEmail = await GetUserEmailAsync(int.Parse(userId));
            string userName = await GetUserNameAsync(int.Parse(userId));
            if (string.IsNullOrEmpty(userEmail))
            {
                TempData["ErrorMessage"] = "Unable to fetch user email.";
                return RedirectToAction("CartList");
            }

            // Create OrderModel
            var order = new OrderModel
            {
                OrderId = 0, // New order
                OrderNumber = GenerateOrderNumber(), // Generate unique order number
                UserId = int.Parse(userId),
                UserName = userName,
                OrderDate = DateTime.UtcNow,
                TotalAmount = grandTotal,
                Status = "Pending", // Default status
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            // Save the order
            var json = JsonConvert.SerializeObject(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage orderResponse = await _httpClient.PostAsync($"{_baseAddress}/Orders", content);

            if (orderResponse.IsSuccessStatusCode)
            {
                // Send email notification
                string subject = "Your Order Confirmation - Tech Verse";
                string message = $@"
Dear {userName ?? userEmail},

Thank you for shopping with Tech Verse! We’re pleased to confirm your order. Below are the details of your purchase:

Order Number: {order.OrderNumber}
Order Date: {order.OrderDate.ToString("dd MMM yyyy")}
Total Products: {totalProducts}
Grand Total: {grandTotal.ToString("C", new System.Globalization.CultureInfo("en-IN")).Replace("₹", "")} /rs

Your order is being processed, and we’ll notify you once it has shipped. If you have any questions, feel free to contact us at support@techverse.com or call us at +919977882200.

Thank you for choosing Tech Verse!

Best regards,  
The Tech Verse Team  
support@techverse.com | www.techverse.com
";

                try
                {
                    _mailService.SendEmailNotification(userEmail, subject, message);
                    TempData["SuccessMessage"] = "Order placed successfully! Check your email for confirmation.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Order placed, but failed to send email: {ex.Message}";
                }

                // Clear the cart
                foreach (var cart in cartList)
                {
                    await _httpClient.DeleteAsync($"{_baseAddress}/Cart/{cart.CartId}");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Error saving order.";
            }

            return RedirectToAction("CartList");
        }

        private async Task<string> GetUserEmailAsync(int userId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAddress}/Users/{userId}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(data);
                return user?.Email;
            }
            return null;
        }

        private async Task<string> GetUserNameAsync(int userId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAddress}/Users/{userId}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserModel>(data);
                return user?.Name; // Assuming UserModel has UserName
            }
            return null;
        }

        private string GenerateOrderNumber()
        {
            // Simple logic to generate ORN### format (e.g., ORN001)
            Random random = new Random();
            int number = random.Next(1, 1000); // Adjust range as needed
            return $"ORN{number:000}";
        }

        private async Task LoadProductDropdownList()
        {
            var response = await _httpClient.GetAsync($"{_baseAddress}/Products/Drop-Down");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<ProductDropdownModel>>(data);
                ViewBag.ProductList = products;
            }
        }

        private async Task LoadUserDropdownList()
        {
            var response = await _httpClient.GetAsync($"{_baseAddress}/Users/Drop-Down");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserDropDownModel>>(data);
                ViewBag.UserList = users;
            }
        }
    }

  
}