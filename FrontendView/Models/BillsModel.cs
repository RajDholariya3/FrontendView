using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendView.Model
{
    public class BillsModel
    {
        public int? BillId { get; set; }

        [Required(ErrorMessage = "Bill Number is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Bill Number must be between 1 and 50 characters.")]
        public string BillNumber { get; set; }

        [Required(ErrorMessage = "Bill Date is required.")]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(BillsModel), nameof(ValidateBillDate))]
        public DateTime BillDate { get; set; }

        [Required(ErrorMessage = "Shipping Address is required.")]
        [StringLength(250, ErrorMessage = "Shipping Address cannot exceed 250 characters.")]
        public string ShippingAddress { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int? UserId { get; set; }

        public string? UserName { get; set; }
        [Required]
        public int? ProductId { get; set; }

        public string? ProductName { get; set; }

        public string? OrderNumber { get; set; }

        [Required(ErrorMessage = "Order ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Order ID must be a positive integer.")]
        public int? OrderId { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public static ValidationResult ValidateBillDate(DateTime billDate, ValidationContext context)
        {
            if (billDate > DateTime.Now)
            {
                return new ValidationResult("Bill Date cannot be in the future.");
            }
            return ValidationResult.Success;
        }
    }
}
