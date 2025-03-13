using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Model;
using System.Net.Http.Headers;

namespace FrontendView.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5012/api") };

            string token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

     
        [HttpGet]
        public async Task<IActionResult> OrderList()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Login");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_httpClient.BaseAddress}/Orders/{userId}");
            HttpResponseMessage response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API call failed with status code: {response.StatusCode}");
                TempData["ErrorMessage"] = "Failed to retrieve orders.";
                return View(new List<OrderModel>());
            }

            string data = await response.Content.ReadAsStringAsync();

            try
            {
                List<OrderModel> orderList;

                if (data.Trim().StartsWith("{")) 
                {
                    var singleOrder = JsonConvert.DeserializeObject<OrderModel>(data);
                    orderList = new List<OrderModel> { singleOrder };  // Convert it to a list
                }
                else if (data.Trim().StartsWith("["))  
                {
                    orderList = JsonConvert.DeserializeObject<List<OrderModel>>(data);
                }
                else
                {
                    throw new JsonSerializationException("Unexpected JSON format");
                }

                return View(orderList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deserialization error: {ex.Message}");
                TempData["ErrorMessage"] = "Error processing order data.";
                return View(new List<OrderModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Save(OrderModel order)
        {
            if (ModelState.IsValid)
            {
                order.OrderId = order.OrderId > 0 ? order.OrderId : 0;
                var json = JsonConvert.SerializeObject(order);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (order.OrderId == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Orders", content);
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Orders/{order.OrderId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("OrderList");
                }

                ViewBag.ErrorMessage = "Error saving order.";
            }
            return View("AddOrder", order);
        }

    }
}
