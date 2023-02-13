using System;

namespace GreenStop.API.Models.ViewModels
{
    public class ItemDetailsViewModel
    {
        public Int32 id{get;set;}
        public String name{get;set;}
        public Double price {get;set;}
        public Int32 quantity{get;set;}
        public String note{get;set;}
    }
}