using System;
using System.ComponentModel.DataAnnotations;


namespace API.MyRestaurant.ViewModels
{
    public class MenuItemsAPIViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Item name is required.")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryID { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    }

