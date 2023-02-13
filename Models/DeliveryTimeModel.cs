using System;
using System.ComponentModel.DataAnnotations;

namespace GreenStop.API.Models
{
    public class DeliveryTimeModel
    {
        [Key]
        public Int32 DeliveryTimeId{get;set;}
        [Required]
        [MaxLength(20)]
        public String TimeSlot{get;set;}
    }
}