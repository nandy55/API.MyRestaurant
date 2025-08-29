using System.ComponentModel.DataAnnotations;

namespace API.MyRestaurant.ViewModels
{
    public class UserTypesAPIViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "User type is required.")]
        [StringLength(50, ErrorMessage = "User type can't exceed 50 characters.")]
        public string Types { get; set; }
    }
}
