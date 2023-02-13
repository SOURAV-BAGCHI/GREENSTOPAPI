using System;

namespace GreenStop.API.Models.ViewModels.StoredProcedureViewModel
{
    public class UserDetailsViewModel
    {
        public String Id{get;set;}
        public String DisplayName{get;set;}
        public String Role{get;set;}
        public String RefreshToken{get;set;}
    }
}