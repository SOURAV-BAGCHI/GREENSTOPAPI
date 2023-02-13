using System;
using System.ComponentModel.DataAnnotations;

namespace GreenStop.API.Models
{
    public class UserOTPStoreModel
    {
        [Key]
        [MaxLength(15)]
        public String Phone{get;set;}
        [Required]
        [MaxLength(6)]
        public String OTP{get;set;}
        [Required]
        public Int32 Type{get;set;}
        [Required]
        public DateTime ExpiryTime{get;set;}
    }
}