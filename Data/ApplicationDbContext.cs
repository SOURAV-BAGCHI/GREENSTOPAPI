using GreenStop.API.Models;
using GreenStop.API.Models.ViewModels;
using GreenStop.API.Models.ViewModels.StoredProcedureViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ViewModels.StoredProcedureViewModel;

namespace GreenStop.API.Data
{
    public class ApplicationDbContext:IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Seed();
            builder.Entity<CatagoryItemDetailsViewModel>().HasNoKey().ToView(null);
            builder.Entity<UserValidityCheckViewModel>().HasNoKey().ToView(null);
            builder.Entity<UserDetailsViewModel>().HasNoKey().ToView(null);
            builder.Entity<RefreshTokenSPViewModel>().HasNoKey().ToView(null);
            builder.Entity<GeneralListViewModel>().HasNoKey().ToView(null);
            builder.Entity<OrderKitchenModel>().HasKey(o => new { o.OrderId, o.KitchenUserId });
            builder.Entity<OrderDeliveryModel>().HasKey(o => new { o.OrderId, o.DeliveryUserId });
            builder.Entity<OrderListViewModel>().HasNoKey().ToView(null);
            builder.Entity<EmployeeDetailsViewModel>().HasNoKey().ToView(null);
            builder.Entity<CartAvailabilityViewModel>().HasNoKey().ToView(null);
        }

        public DbSet<TokenModel> Tokens{get;set;}
        public DbSet<UserOTPStoreModel> UserOTPStore{get;set;}
        public DbSet<CatagoryModel> CatagoryDetails{get;set;}
        public DbSet<ItemModel> ItemDetails{get;set;}
        public DbSet<DeliveryTimeModel> DeliveryTimeDetails{get;set;}
        public DbSet<OrderModel> OrderDetails{get;set;}
        public DbSet<UserAddressModel> AddressDetails{get;set;}
        public DbSet<UserFCMModel> UserFCMDetails{get;set;}
        public DbSet<ServerFCMModel> ServerFCMDetails{get;set;}
        public DbSet<OrderKitchenModel> OrderKitchenDetails{get;set;}
        public DbSet<OrderDeliveryModel> OrderDeliveryDetails{get;set;}
        public DbSet<UserFeedbackModel> UserFeedbacks{get;set;}
    }
}