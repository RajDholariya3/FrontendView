using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Model;
using static FrontendView.Models.DropDownModel;
using System.Net.Http.Headers;

namespace FrontendView.Controllers
{
    public class PaymentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;

        public PaymentController(IHttpContextAccessor httpContextAccessor)
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

        // Get list of payments
        [HttpGet]
        public async Task<IActionResult> PaymentList()
        {
            List<PaymentsModel> paymentsList = new List<PaymentsModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Payments");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                paymentsList = JsonConvert.DeserializeObject<List<PaymentsModel>>(data);
            }
            return View(paymentsList);
        }

        // Get a single payment for add/edit
        [HttpGet]
        public async Task<IActionResult> AddPayment(int? paymentId)
        {
            await LoadOrderDropdownList();
            await LoadUserDropdownList();
            if (paymentId.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Payments/{paymentId}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var payment = JsonConvert.DeserializeObject<PaymentsModel>(data);
                    return View(payment);
                }
            }
            return View(new PaymentsModel()); // If no paymentId, render an empty payment model (for add)
        }

        // Save payment (Create or Update)
        [HttpPost]
        public async Task<IActionResult> Save(PaymentsModel payment)
        {
            if (ModelState.IsValid)
            {
                // Set PaymentId to 0 for new payments
                payment.PaymentId = payment.PaymentId > 0 ? payment.PaymentId : 0;
                var json = JsonConvert.SerializeObject(payment);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (payment.PaymentId == 0)
                {
                    // Create new payment
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Payments", content);
                }
                else
                {
                    // Update existing payment
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Payments/{payment.PaymentId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("PaymentList"); // Redirect to payment list after saving
                }

                ViewBag.ErrorMessage = "Error saving payment.";
            }
            return View("AddPayment", payment); // Show the add/edit page again if validation fails
        }

        // Delete a payment
        public async Task<IActionResult> Delete(int paymentId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Payments/{paymentId}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Payment deleted successfully.";
                    return RedirectToAction("PaymentList"); // Redirect to payment list after successful deletion
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Failed to delete payment. Status: {response.StatusCode} - {response.ReasonPhrase}. Details: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["ErrorMessage"] = "An unexpected error occurred while deleting the payment. Please try again later.";
            }

            return RedirectToAction("PaymentList"); // Return to payment list on failure
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


        private async Task LoadUserDropdownList()
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Users/Drop-Down");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserDropDownModel>>(data);
                ViewBag.UserList = users;
            }
        }

    }
}
