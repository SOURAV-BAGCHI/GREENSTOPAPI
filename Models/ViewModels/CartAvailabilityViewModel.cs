using System;

namespace GreenStop.API.Models.ViewModels
{
    public class CartAvailabilityViewModel
    {
        public Int32 Id{get;set;}
        public Double Price{get;set;}
        public Boolean Available{get;set;}
    }
}