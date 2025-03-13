using System.ComponentModel.DataAnnotations;

namespace FrontendView.Model
{
    public class CustomerModel
    {
        public int? CustomerId { get; set; }
        [Required(ErrorMessage = "User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive integer.")]
        public int UserId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Loyalty Points must be non-negative.")]
        public int LoyaltyPoint { get; set; }

        [Required(ErrorMessage = "Membership Type is required.")]
        [StringLength(50, ErrorMessage = "Membership Type cannot exceed 50 characters.")]
        public string MembershipType { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? UserName { get;  set; }
    }
}
