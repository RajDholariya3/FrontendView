using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Model;
using static FrontendView.Models.DropDownModel;
using System.Net.Http.Headers;

namespace FrontendView.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;

        public OrderDetailsController(IHttpContextAccessor httpContextAccessor)
        {
            _baseAddress = new Uri("http://localhost:5012/api"); // Replace with your actual API base address
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

        // Get list of order details for a specific order
        [HttpGet]
        public async Task<IActionResult> OrderDetailsList(int orderId)
        {
            List<OrderDetailsModel> orderDetailsList = new List<OrderDetailsModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/OrderDetails");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                orderDetailsList = JsonConvert.DeserializeObject<List<OrderDetailsModel>>(data);
            }
            return View(orderDetailsList);
        }

        // Get a single order detail for add/edit
        [HttpGet]
        public async Task<IActionResult> AddOrderDetail(int? orderDetailId)
        {
            await LoadOrderDropdownList();
            await LoadProductDropdownList();
            if (orderDetailId.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/OrderDetails/{orderDetailId}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var orderDetail = JsonConvert.DeserializeObject<OrderDetailsModel>(data);
                    return View(orderDetail);
                }
            }
            return View(new OrderDetailsModel()); // If no orderDetailId, render an empty order detail model (for add)
        }

        // Save order detail (Create or Update)
        [HttpPost]
        public async Task<IActionResult> Save(OrderDetailsModel orderDetail)
        {
            if (ModelState.IsValid)
            {
                // Set OrderDetailId to 0 for new order details
                orderDetail.OrderDetailId = orderDetail.OrderDetailId > 0 ? orderDetail.OrderDetailId : 0;
                var json = JsonConvert.SerializeObject(orderDetail);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (orderDetail.OrderDetailId == 0)
                {
                    // Create new order detail
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/OrderDetails", content);
                }
                else
                {
                    Console.WriteLine("From edit");
                    // Update existing order detail
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/OrderDetails/{orderDetail.OrderDetailId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("OrderDetailsList", new { orderId = orderDetail.OrderId }); // Redirect to the order details list after saving
                }

                ViewBag.ErrorMessage = "Error saving order detail.";
            }
            return View("AddOrderDetail", orderDetail); // Show the add/edit page again if validation fails
        }

        // Delete an order detail
        public async Task<IActionResult> Delete(int orderDetailId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/OrderDetails/{orderDetailId}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Order detail deleted successfully.";
                    return RedirectToAction("OrderDetailsList"); // Redirect to order details list after successful deletion
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to delete order detail. Status: {response.StatusCode} - {response.ReasonPhrase}. Details: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the order detail. Please try again later.";
            }

            return RedirectToAction("OrderDetailsList"); // Return to order details list on failure
        }

        private async Task LoadProductDropdownList()
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Products/Drop-Down");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<List<ProductDropdownModel>>(data);
                ViewBag.ProductList = products;
            }
        }
        private async Task LoadOrderDropdownList()
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Orders/Drop-Down");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var orders = JsonConvert.DeserializeObject<List<OrderDropdownModel>>(data);
                ViewBag.OrderList = orders;
            }
        }
    }
}
