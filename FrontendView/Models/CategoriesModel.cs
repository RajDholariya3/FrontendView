using System.ComponentModel.DataAnnotations;

namespace FrontendView.Models
{
    public class CategoriesModel
    {
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category Name must be between 1 and 100 characters.")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
