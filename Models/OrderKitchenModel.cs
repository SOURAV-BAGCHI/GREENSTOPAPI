using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class OrderKitchenModel
    {
        [Required]
        [MaxLength(30)]
        public String OrderId {get;set;}
        [Required]
        public String KitchenUserId{get;set;}
        [Required]
        public DateTime CreateDate{get;set;}
        [Required]
        public Int32 Status {get;set;}  // 1 for current and 2 for history
        [Required]
        public DateTime DeliveryDate{get;set;}
        [Required]
        public Int32 DeliveryTimeId{get;set;}
    }
}