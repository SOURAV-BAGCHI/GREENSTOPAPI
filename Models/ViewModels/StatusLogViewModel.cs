using System;

namespace GreenStop.API.Models.ViewModels
{
    public class StatusLogViewModel
    {
        public Int32 Status{get;set;}
        public String By{get;set;}
        public DateTime On{get;set;}
        public String To{get;set;}
    }
}