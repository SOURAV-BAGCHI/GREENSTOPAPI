using System;
using System.Collections.Generic;

namespace Models.ViewModels
{
    public class OrderDetailsSmall
    {
        public List<String> name_list{get;set;}
        public List<String> note_list{get;set;}
        public Double total{get;set;}
        public Int32 status{get;set;}
        public DateTime orderdate{get;set;}
        public DateTime deliverydate{get;set;}
        public String deliverytime{get;set;}
        public String orderid{get;set;}
        public String code{get;set;}
        public List<Int32> qty{get;set;}
    }
}