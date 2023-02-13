using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace GreenStop.API.Models
{
    public class ItemModel
    {
        [Key]
        public Int32 Id{get;set;}
        [Required]
        public Int32 CatagoryId{get;set;}
        [Required]
        [MaxLength(50)]
        public String Name1{get;set;}
        [Required]
        [MaxLength(50)]
        public String Name2{get;set;}
        public String Detail{get;set;}
        public String Detail2{get;set;}
        [Required]
        public Int32 Type{get;set;}
        [Required]
        public String Description{get;set;}
        public String Description2{get;set;}
        public Double Price{get;set;}
        public Boolean Available{get;set;}
    }
}