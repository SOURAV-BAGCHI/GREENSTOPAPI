using System;

namespace Models.ViewModels
{
    public class PaymentOrderInfoViewModel
    {
        public Int32 Mode{get;set;}
        public Double Amt{get;set;}
        public String TId{get;set;}
        public String OrderId{get;set;}
    }
}