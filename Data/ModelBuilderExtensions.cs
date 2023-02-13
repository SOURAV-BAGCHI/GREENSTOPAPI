using GreenStop.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GreenStop.API.Data
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new {Id="1",Name="Customer",NormalizedName="CUSTOMER"},
                new {Id="2",Name="ITAdmin",NormalizedName="ITADMIN"},
                new {Id="3",Name="ChiefManager",NormalizedName="CHIEFMANAGER"},
                new {Id="4",Name="CustomerService",NormalizedName="CUSTOMERSERVICE"},
                new {Id="5",Name="KitchenManager",NormalizedName="KITCHENMANAGER"},
                new {Id="6",Name="Delivery",NormalizedName="DELIVERY"}
            );
            builder.Entity<DeliveryTimeModel>().HasData(
                new{DeliveryTimeId=1,TimeSlot="11 am - 12 pm"},
                new{DeliveryTimeId=2,TimeSlot="12 pm - 1 pm"},
                new{DeliveryTimeId=3,TimeSlot="1 pm - 2 pm"},
                new{DeliveryTimeId=4,TimeSlot="2 pm - 3 pm"},
                new{DeliveryTimeId=5,TimeSlot="3 pm - 4 pm"},
                new{DeliveryTimeId=6,TimeSlot="4 pm - 5 pm"},
                new{DeliveryTimeId=7,TimeSlot="5 pm - 6 pm"},
                new{DeliveryTimeId=8,TimeSlot="6 pm - 7 pm"},
                new{DeliveryTimeId=9,TimeSlot="7 pm - 8 pm"},
                new{DeliveryTimeId=10,TimeSlot="8 pm - 9 pm"},
                new{DeliveryTimeId=11,TimeSlot="9 pm - 10 pm"}
            );
        }
    }
}