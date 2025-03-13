using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Model;
using static FrontendView.Models.DropDownModel;
using System.Net.Http.Headers;

namespace FrontendView.Controllers
{
    public class CustomerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;

        public CustomerController(IHttpContextAccessor httpContextAccessor)
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
        public async Task<IActionResult> CustomerList()
        {
            List<CustomerModel> customerList = new List<CustomerModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Customer");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                customerList = JsonConvert.DeserializeObject<List<CustomerModel>>(data);
            }
            return View(customerList);
        }

        [HttpGet]
        public async Task<IActionResult> AddCustomer(int? CustomerID)
        {
            await LoadUserDropdownList();

            if (CustomerID.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Customer/{CustomerID}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var customer = JsonConvert.DeserializeObject<CustomerModel>(data);
                    return View(customer);
                }
            }
            return View(new CustomerModel());
        }

        [HttpPost]
        public async Task<IActionResult> Save(CustomerModel customer)
        {
            if (ModelState.IsValid)
            {
                customer.CustomerId = customer.CustomerId > 0 ? customer.CustomerId : 0;
                var json = JsonConvert.SerializeObject(customer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (customer.CustomerId == 0)
                {
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Customer", content);
                }
                else
                {
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Customer/{customer.CustomerId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("CustomerList");
                }

                ViewBag.ErrorMessage = "Error saving customer.";
            }
            return View("AddCustomer", customer);
        }

        public async Task<IActionResult> Delete(int CustomerID)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Customer/{CustomerID}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Customer deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error deleting customer: {response.StatusCode} - {response.ReasonPhrase}\nDetails: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the customer: {ex.Message}";
            }
            return RedirectToAction("CustomerList");
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
