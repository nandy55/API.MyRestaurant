using System.ComponentModel.DataAnnotations;
using System;
namespace API.MyRestaurant.ViewModels
{
    public class MenuCategoriesAPIViewModel
    {
        
            public int ID { get; set; }

            [Required(ErrorMessage = "Category Name is required.")]
            public string CategoryName { get; set; }
        }

    }

