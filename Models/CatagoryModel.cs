using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GreenStop.API.Models
{
    public class CatagoryModel
    {
        [Key]
        public Int32 Id {get;set;}
        [Required]
        [MaxLength(30)]
        public String Name{get;set;}
        [Required]
        public String Image{get;set;}
        [Required]
        public Int32 Priority{get;set;}
        public Boolean Available{get;set;}
    }
}