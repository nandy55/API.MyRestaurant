using System.ComponentModel.DataAnnotations;

namespace API.MyRestaurant.ViewModels
{
    public class UserDetailsAPIViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "UserID is required.")]
        public string UserID { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address1 { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }


        [Required(ErrorMessage = "UserTypesID is required.")]
        public int UserTypesID { get; set; }

        [Required(ErrorMessage = "RestaurantID is required.")]
        public int RestaurantID { get; set; }

        public bool IsActive { get; set; } = true;

    }
}
