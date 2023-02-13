using System;
using System.ComponentModel.DataAnnotations;

namespace GreenStop.API.Models
{
    public class ServerFCMModel
    {
        [Key]
        public Int32 Id{get;set;}
        [Required]
        public String ServerApiKey{get;set;}
        [Required]
        public String SenderId{get;set;}
    }
}