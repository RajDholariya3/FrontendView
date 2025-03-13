using System;
using System.ComponentModel.DataAnnotations;

namespace FrontendView.Model
{
    public class PaymentsModel
    {
        public int? PaymentId { get; set; }

        [Required(ErrorMessage = "Order ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Order ID must be a positive integer.")]
        public int OrderId { get; set; }

        public string? OrderNumber { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int UserId { get; set; }
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Payment Date is required.")]
        [DataType(DataType.Date)]
        
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Amount Paid is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount Paid must be a non-negative value.")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Payment Method is required.")]
        [MaxLength(50, ErrorMessage = "Payment Method cannot exceed 50 characters.")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Payment Status is required.")]
        [MaxLength(50, ErrorMessage = "Payment Status cannot exceed 50 characters.")]
        public string PaymentStatus { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Custom validation method for PaymentDate
       
    }
}
