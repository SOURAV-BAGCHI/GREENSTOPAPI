using System;
using System.ComponentModel.DataAnnotations;

namespace GreenStop.API.Models
{
    public class OrderModel
    {
        [Key]
        [MaxLength(30)]
        public String OrderId {get;set;}
        [Required]
        [MaxLength(450)]
        public String UserId{get;set;}
        [Required]
        public String CustomerInfo{get;set;}
        [Required]
        public String ItemDetails{get;set;}
        [Required]
        public DateTime CreateDate{get;set;}
        [Required]
        public Int32 OrderStatus{get;set;}
        [Required]
        public String OrderStatusLog{get;set;}
        [Required]
        public Boolean PaymentStatus{get;set;}
        [Required]
        public DateTime DeliveryDate{get;set;}
        [Required]
        public Int32 DeliveryTimeId{get;set;}
        public String PaymentInfo{get;set;}
        [Required]
        public Double SubTotal{get;set;}
        [Required]
        public Double Tax{get;set;}
        [Required]
        public Double DeliveryCharges{get;set;}
        [Required]
        public String CustomerServiceStatus{get;set;}
    }
}