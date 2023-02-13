using System;
using System.ComponentModel.DataAnnotations;

namespace GreenStop.API.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name="Phone Number")]
        public String Phone { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        public String Role{get;set;}
        public String DisplayName{get;set;}
    }
}