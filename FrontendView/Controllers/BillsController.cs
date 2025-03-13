using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using FrontendView.Model;
using static FrontendView.Models.DropDownModel;
using NuGet.Packaging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Net.Http.Headers;

namespace FrontendView.Controllers
{
    public class BillsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseAddress;

        public BillsController(IHttpContextAccessor httpContextAccessor)
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
        public async Task<IActionResult> BillList()
        {
            List<BillsModel> billsList = new List<BillsModel>();
            HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Bill");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                billsList = JsonConvert.DeserializeObject<List<BillsModel>>(data);
            }
            return View(billsList);
        }

        [HttpGet]
        public async Task<IActionResult> AddBill(int? BillID)
        {
            await LoadProductDropdownList();
            await LoadOrderDropdownList();
            await LoadUserDropdownList();

            if (BillID.HasValue)
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Bill/{BillID}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var bill = JsonConvert.DeserializeObject<BillsModel>(data);
                    return View(bill);
                }
            }
            return View(new BillsModel());
        }

        [HttpPost]
        public async Task<IActionResult> Save(BillsModel bill)
        {
            if (ModelState.IsValid)
            {
                bill.BillId = bill.BillId > 0 ? bill.BillId : 0; // Removed extra semicolon
                var json = JsonConvert.SerializeObject(bill);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                if (bill.BillId == 0)
                {
                    // Create new bill
                    response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Bill", content);
                }
                else
                {
                    // Update existing bill
                    response = await _httpClient.PutAsync($"{_httpClient.BaseAddress}/Bill/{bill.BillId}", content);
                }

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("BillList");
                }

                ViewBag.ErrorMessage = "Error saving bill.";
            }
            return View("AddBill", bill);
        }

        public async Task<IActionResult> Delete(int BillID)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/Bill/{BillID}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Bill deleted successfully.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error deleting bill: {response.StatusCode} - {response.ReasonPhrase}\nDetails: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the bill: {ex.Message}";
            }
            return RedirectToAction("BillList");
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
