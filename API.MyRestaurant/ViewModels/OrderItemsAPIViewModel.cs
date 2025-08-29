using System.ComponentModel.DataAnnotations;
using System;

namespace API.MyRestaurant.ViewModels
{
    public class OrderItemsAPIViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Order ID is required.")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Menu Item ID is required.")]
        public int MenuItemID { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Item Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Item Price must be greater than 0.")]
        public decimal ItemPrice { get; set; }
    }
}
