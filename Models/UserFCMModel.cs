using System;
using System.ComponentModel.DataAnnotations;

namespace GreenStop.API.Models
{
    public class UserFCMModel
    {
        [Key]
        public Int64 FCMDetailsId{get;set;}
        [Required]
        [MaxLength(450)]
        public String UserId{get;set;}
        [Required]
        public String FCMToken{get;set;}
    }
}