namespace FrontendView.Models
{
    public class CartModel
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public string? Name { get; set; } // Nullable in case username is not always needed

        public int ProductId { get; set; }
        public string? ProductName { get; set; } // Nullable if product name is retrieved later

        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
