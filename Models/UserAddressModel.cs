using System;
using System.ComponentModel.DataAnnotations;

namespace GreenStop.API.Models
{
    public class UserAddressModel
    {
        [Key]
        public Int64 AddressId{get;set;}
        [Required]
        [MaxLength(450)]
        public String UserId{get;set;}
        [Required]
        [MaxLength(50)]
        public String Name{get;set;}
        [Required]
        [MaxLength(200)]
        public String AddressLine1{get;set;}
        [MaxLength(200)]
        public String AddressLine2{get;set;}
        [Required]
        [MaxLength(200)]
        public String Landmark{get;set;}
        [Required]
        [MaxLength(15)]
        public String Phone{get;set;}
        [Required]
        [MaxLength(6)]
        public String Pincode{get;set;}
        [MaxLength(15)]
        public String AlternatePhone{get;set;}
    }
}