using System;
using System.ComponentModel.DataAnnotations;

namespace API.MyRestaurant.ViewModels
{
    public class RestaurantsAPIViewModel
    {
        public int Id { get; set; } // Primary key, not required in creation

        [Required(ErrorMessage = "Restaurant name is required.")]
        public string RestaurantName { get; set; }

        public string? Location { get; set; } // Nullable, no [Required]

        [Phone(ErrorMessage = "Invalid contact number.")]
        public string? ContactNumber { get; set; } // Optional phone validation

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        public TimeSpan? OpeningTime { get; set; }

        public TimeSpan? ClosingTime { get; set; }

        public bool IsActive { get; set; } = true; // Default true
    }
}

