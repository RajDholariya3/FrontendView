namespace FrontendView.Models
{
    public class DropDownModel
    {
        public class OrderDropdownModel
        {
            public int OrderId { get; set; }
            public string OrderNumber { get; set; }

        }
        public class UserDropDownModel
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
        }
        public class ProductDropdownModel
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }

        }
        public class CategoriesDropdownModel
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
        }

    }
}
