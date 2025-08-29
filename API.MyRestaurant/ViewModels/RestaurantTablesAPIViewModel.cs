using System;
using System.ComponentModel.DataAnnotations;


namespace API.MyRestaurant.ViewModels
{
    public class RestaurantTablesAPIViewModel
    {
        public int ID { get; set; } // Primary key, not required in creation

        [Required(ErrorMessage = "Table number is required.")]
        public int TableNumber { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 100, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } // Example values: "Available", "Occupied", "Reserved"
    }
}
