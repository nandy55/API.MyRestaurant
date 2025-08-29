using System;
using System.ComponentModel.DataAnnotations;


namespace API.MyRestaurant.ViewModels
{
    public class PaymentsAPIViewModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Order ID is required.")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Payment Type is required.")]
        [StringLength(50, ErrorMessage = "Payment Type cannot exceed 50 characters.")]
        public string PaymentType { get; set; }

        [Required(ErrorMessage = "Payment Date is required.")]
        public DateTime PaymentDate { get; set; }
    }

}

