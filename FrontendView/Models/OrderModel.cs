using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FrontendView.Model
{
    public class OrderModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Order Number is required.")]
        [RegularExpression(@"^ORN\d{3}$", ErrorMessage = "Order Number must be in the format 'ORN###' (e.g., ORN001).")]
        public string OrderNumber { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int UserId { get; set; }
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Order Date is required.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(OrderModel), nameof(ValidateOrderDate))]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Total Amount is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Total Amount must be a non-negative value.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Custom validation method for OrderDate
        public static ValidationResult ValidateOrderDate(DateTime orderDate, ValidationContext context)
        {
            if (orderDate > DateTime.Now)
            {
                return new ValidationResult("Order Date cannot be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}
