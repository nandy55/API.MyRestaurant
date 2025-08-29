using System;
using System.ComponentModel.DataAnnotations;

namespace API.MyRestaurant.ViewModels
{
    public class OrdersAPIViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Table ID is required.")]
        public int TableID { get; set; }

        [Required(ErrorMessage = "Staff ID is required.")]
        public int StaffID { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now; // Optional: Default to now

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;
    }
}
