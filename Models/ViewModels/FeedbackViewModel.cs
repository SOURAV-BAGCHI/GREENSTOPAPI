using System;

namespace GreenStop.API.Models.ViewModels
{
    public class FeedbackViewModel
    {
        public Int16 Rating{get;set;}
        public String ReviewTitle{get;set;}
        public String Review{get;set;}
        public String OrderId{get;set;}
    }
}