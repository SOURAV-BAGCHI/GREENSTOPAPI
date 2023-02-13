using System;

namespace Models.ViewModels.StoredProcedureViewModel
{
    public class OrderListViewModel
    {
        public String OrderId{get;set;}
        public String CustomerInfo{get;set;}
        public String ItemDetails{get;set;}
        public Int32 OrderStatus{get;set;}
        public String DeliveryDate{get;set;}
        public String DeliveryTime{get;set;}
    }
}