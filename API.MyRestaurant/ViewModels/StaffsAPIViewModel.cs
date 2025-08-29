using System.ComponentModel.DataAnnotations;

namespace API.MyRestaurant.ViewModels
{
    public class StaffsAPIViewModel
    {
        public int Id { get; set; }
        public int UserDetailsID { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
        
    }
}
